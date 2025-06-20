using Facelift_App.Helper;
using Facelift_App.Services;
using NLog;
using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Facelift_App.Repositories
{
    public class PalletRepository : IPallets
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<MsPallet> GetDataByTagIdAsync(string TagId)
        {
            MsPallet data = null;
            try
            {
                data = await db.MsPallets.FindAsync(TagId);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<MsPallet> GetDataByPalletCodeAsync(string PalletCode)
        {
            MsPallet data = null;
            try
            {
                data = await db.MsPallets.Where(s => s.PalletCode.ToLower().Equals(PalletCode.ToLower())).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<MsPallet> GetDataByTagWarehouseIdAsync(string TagId, string WarehouseId)
        {
            MsPallet data = null;
            try
            {
                data = await db.MsPallets.Where(s => s.TagId.Equals(TagId) && s.WarehouseId.Equals(WarehouseId)).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public IEnumerable<VwPallet> GetTotalStock(string OriginId, string PalletCondition, string search, string sortDirection, string sortColName)
        {
            IQueryable<VwPallet> query = db.VwPallets.AsQueryable().Where(m => m.OriginId.Equals(OriginId));
            IEnumerable<VwPallet> list = null;
            try
            {
                query = query
                        .Where(m => m.TagId.Contains(search) ||
                           m.PalletCode.Contains(search) || m.PalletName.Contains(search) || m.PalletCondition.Contains(search) ||
                           m.WarehouseName.Contains(search) || m.PalletOwner.Contains(search) || m.PalletProducer.Contains(search) ||
                           m.RegisteredBy.Contains(search) || m.PalletMovementStatus.Contains(search) || m.LastTransactionName.Contains(search) ||
                           m.LastTransactionCode.Contains(search));

                if (!string.IsNullOrEmpty(PalletCondition))
                {
                    query = query.Where(m => m.PalletCondition.Equals(PalletCondition));
                }

                //columns sorting
                Dictionary<string, Func<VwPallet, object>> cols = new Dictionary<string, Func<VwPallet, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletCode", x => x.PalletCode);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletCondition", x => x.PalletCondition);
                cols.Add("WarehouseName", x => x.WarehouseName);
                cols.Add("PalletOwner", x => x.PalletOwner);
                cols.Add("PalletProducer", x => x.PalletProducer);
                cols.Add("ProducedDate", x => x.ProducedDate);
                cols.Add("RegisteredBy", x => x.RegisteredBy);
                cols.Add("RegisteredAt", x => x.RegisteredAt);
                cols.Add("ReceivedBy", x => x.ReceivedBy);
                cols.Add("ReceivedAt", x => x.ReceivedAt);
                cols.Add("PalletMovementStatus", x => x.PalletMovementStatus);
                cols.Add("LastTransactionName", x => x.LastTransactionName);
                cols.Add("LastTransactionCode", x => x.LastTransactionCode);
                cols.Add("LastTransactionDate", x => x.LastTransactionDate);

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

        public IEnumerable<VwPallet> GetStockReport(string OriginId)
        {
            IQueryable<VwPallet> query = db.VwPallets.AsQueryable().Where(m => m.OriginId.Equals(OriginId));
            IEnumerable<VwPallet> list = Enumerable.Empty<VwPallet>();
            try
            {

                //columns sorting
                Dictionary<string, Func<VwPallet, object>> cols = new Dictionary<string, Func<VwPallet, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletCode", x => x.PalletCode);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletCondition", x => x.PalletCondition);
                cols.Add("WarehouseName", x => x.WarehouseName);
                cols.Add("PalletOwner", x => x.PalletOwner);
                cols.Add("PalletProducer", x => x.PalletProducer);
                cols.Add("ProducedDate", x => x.ProducedDate);
                cols.Add("RegisteredBy", x => x.RegisteredBy);
                cols.Add("RegisteredAt", x => x.RegisteredAt);
                cols.Add("ReceivedBy", x => x.ReceivedBy);
                cols.Add("ReceivedAt", x => x.ReceivedAt);
                cols.Add("PalletMovementStatus", x => x.PalletMovementStatus);
                cols.Add("LastTransactionName", x => x.LastTransactionName);
                cols.Add("LastTransactionCode", x => x.LastTransactionCode);
                cols.Add("LastTransactionDate", x => x.LastTransactionDate);

                list = query.OrderBy(cols["TagId"]);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public IEnumerable<VwPallet> GetStockActualReport(string OriginId)
        {
            IQueryable<VwPallet> query = db.VwPallets.AsQueryable().Where(m => m.WarehouseId.Equals(OriginId) && m.PalletCondition.Equals(Constant.PalletCondition.GOOD.ToString()) && m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString()));
            IEnumerable<VwPallet> list = Enumerable.Empty<VwPallet>();
            try
            {

                //columns sorting
                Dictionary<string, Func<VwPallet, object>> cols = new Dictionary<string, Func<VwPallet, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletCode", x => x.PalletCode);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletCondition", x => x.PalletCondition);
                cols.Add("WarehouseName", x => x.WarehouseName);
                cols.Add("PalletOwner", x => x.PalletOwner);
                cols.Add("PalletProducer", x => x.PalletProducer);
                cols.Add("ProducedDate", x => x.ProducedDate);
                cols.Add("RegisteredBy", x => x.RegisteredBy);
                cols.Add("RegisteredAt", x => x.RegisteredAt);
                cols.Add("ReceivedBy", x => x.ReceivedBy);
                cols.Add("ReceivedAt", x => x.ReceivedAt);
                cols.Add("PalletMovementStatus", x => x.PalletMovementStatus);
                cols.Add("LastTransactionName", x => x.LastTransactionName);
                cols.Add("LastTransactionCode", x => x.LastTransactionCode);
                cols.Add("LastTransactionDate", x => x.LastTransactionDate);

                list = query.OrderBy(cols["TagId"]);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public IEnumerable<VwPalletShipment> GetStockDeliveryReport(string OriginId)
        {
            IQueryable<VwPalletShipment> query = db.VwPalletShipments.AsQueryable().Where(m => m.OriginId.Equals(OriginId));
            IEnumerable<VwPalletShipment> list = Enumerable.Empty<VwPalletShipment>();
            try
            {

                //columns sorting
                Dictionary<string, Func<VwPalletShipment, object>> cols = new Dictionary<string, Func<VwPalletShipment, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletCode", x => x.PalletCode);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletCondition", x => x.PalletCondition);
                cols.Add("WarehouseName", x => x.WarehouseName);
                cols.Add("PalletOwner", x => x.PalletOwner);
                cols.Add("PalletProducer", x => x.PalletProducer);
                cols.Add("ProducedDate", x => x.ProducedDate);
                cols.Add("RegisteredBy", x => x.RegisteredBy);
                cols.Add("RegisteredAt", x => x.RegisteredAt);
                cols.Add("ReceivedBy", x => x.ReceivedBy);
                cols.Add("ReceivedAt", x => x.ReceivedAt);
                cols.Add("PalletMovementStatus", x => x.PalletMovementStatus);
                cols.Add("LastTransactionName", x => x.LastTransactionName);
                cols.Add("LastTransactionCode", x => x.LastTransactionCode);
                cols.Add("LastTransactionDate", x => x.LastTransactionDate);

                list = query.OrderBy(cols["TagId"]);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public int GetTotalStock(string OriginId)
        {
            int totalData = 0;
            try
            {
                IQueryable<VwPallet> query = db.VwPallets.AsQueryable().Where(m => m.OriginId.Equals(OriginId));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public IEnumerable<MsPallet> GetActualStock(string WarehouseId, string PalletCondition, string search, string sortDirection, string sortColName)
        {
            IQueryable<MsPallet> query = db.MsPallets.AsQueryable().Where(m => m.WarehouseId.Equals(WarehouseId) && m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString()));
            IEnumerable<MsPallet> list = null;
            try
            {
                query = query
                        .Where(m => m.TagId.Contains(search) ||
                           m.PalletCode.Contains(search) || m.PalletName.Contains(search) || m.PalletCondition.Contains(search) ||
                           m.WarehouseName.Contains(search) || m.PalletOwner.Contains(search) || m.PalletProducer.Contains(search) ||
                           m.RegisteredBy.Contains(search) || m.PalletMovementStatus.Contains(search) || m.LastTransactionName.Contains(search) ||
                           m.LastTransactionCode.Contains(search));

                if (!string.IsNullOrEmpty(PalletCondition))
                {
                    query = query.Where(m => m.PalletCondition.Equals(PalletCondition));
                }

                //columns sorting
                Dictionary<string, Func<MsPallet, object>> cols = new Dictionary<string, Func<MsPallet, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletCode", x => x.PalletCode);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletCondition", x => x.PalletCondition);
                cols.Add("WarehouseName", x => x.WarehouseName);
                cols.Add("PalletOwner", x => x.PalletOwner);
                cols.Add("PalletProducer", x => x.PalletProducer);
                cols.Add("ProducedDate", x => x.ProducedDate);
                cols.Add("RegisteredBy", x => x.RegisteredBy);
                cols.Add("RegisteredAt", x => x.RegisteredAt);
                cols.Add("ReceivedBy", x => x.ReceivedBy);
                cols.Add("ReceivedAt", x => x.ReceivedAt);
                cols.Add("PalletMovementStatus", x => x.PalletMovementStatus);
                cols.Add("LastTransactionName", x => x.LastTransactionName);
                cols.Add("LastTransactionCode", x => x.LastTransactionCode);
                cols.Add("LastTransactionDate", x => x.LastTransactionDate);

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

        public IEnumerable<MsPallet> GetActualStockReport(string WarehouseId)
        {
            IQueryable<MsPallet> query = db.MsPallets.AsQueryable().Where(m => m.WarehouseId.Equals(WarehouseId) && m.PalletCondition.Equals(Constant.PalletCondition.GOOD.ToString()) && m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString()));
            IEnumerable<MsPallet> list = null;
            try
            {

                //columns sorting
                Dictionary<string, Func<MsPallet, object>> cols = new Dictionary<string, Func<MsPallet, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletCode", x => x.PalletCode);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletCondition", x => x.PalletCondition);
                cols.Add("WarehouseName", x => x.WarehouseName);
                cols.Add("PalletOwner", x => x.PalletOwner);
                cols.Add("PalletProducer", x => x.PalletProducer);
                cols.Add("ProducedDate", x => x.ProducedDate);
                cols.Add("RegisteredBy", x => x.RegisteredBy);
                cols.Add("RegisteredAt", x => x.RegisteredAt);
                cols.Add("ReceivedBy", x => x.ReceivedBy);
                cols.Add("ReceivedAt", x => x.ReceivedAt);
                cols.Add("PalletMovementStatus", x => x.PalletMovementStatus);
                cols.Add("LastTransactionName", x => x.LastTransactionName);
                cols.Add("LastTransactionCode", x => x.LastTransactionCode);
                cols.Add("LastTransactionDate", x => x.LastTransactionDate);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public int GetActualStock(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<MsPallet> query = db.MsPallets.AsQueryable().Where(m => m.WarehouseId.Equals(WarehouseId) && m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString()));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public int GetTotalInShipment(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<MsPallet> query = db.MsPallets.AsQueryable().Where(m => m.WarehouseId.Equals(WarehouseId) && m.PalletCondition.Equals(Constant.PalletCondition.GOOD.ToString()) && !m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString()));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public int GetTotalByCondition(string OriginId, string Condition)
        {
            int totalData = 0;
            try
            {
                IQueryable<VwPallet> query = db.VwPallets.AsQueryable().Where(m => m.OriginId.Equals(OriginId) 
                && m.PalletCondition.Equals(Condition));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public int GetActualByCondition(string WarehouseId, string Condition)
        {
            int totalData = 0;
            try
            {
                IQueryable<MsPallet> query = db.MsPallets.AsQueryable().Where(m => m.WarehouseId.Equals(WarehouseId)
                && m.PalletCondition.Equals(Condition)
                && m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString()));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public IEnumerable<VwPalletWarehouse> GetWarehouseStock(string OriginId)
        {
            IQueryable<VwPalletWarehouse> query = db.VwPalletWarehouses.AsQueryable().Where(m => m.OriginId.Equals(OriginId));
            IEnumerable<VwPalletWarehouse> list = new List<VwPalletWarehouse>();
            try
            {
                list = query.OrderBy(x => x.WarehouseCode).ToList();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list;
        }

        public IEnumerable<VwPalletWarehouseCondition> GetWarehouseStockByCondition(string OriginId, string PalletCondition)
        {
            IQueryable<VwPalletWarehouseCondition> query = db.VwPalletWarehouseConditions.AsQueryable().Where(m => m.OriginId.Equals(OriginId) && m.PalletCondition.Equals(PalletCondition));
            IEnumerable<VwPalletWarehouseCondition> list = new List<VwPalletWarehouseCondition>();
            try
            {
                list = query.OrderBy(x => x.WarehouseCode).ToList();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list;
        }

        public IEnumerable<VwPalletShipment> GetDeliveryStock(string OriginId, string search, string sortDirection, string sortColName)
        {
            IQueryable<VwPalletShipment> query = db.VwPalletShipments.AsQueryable().Where(m => m.OriginId.Equals(OriginId));
            IEnumerable<VwPalletShipment> list = null;
            try
            {
                query = query
                        .Where(m => m.TagId.Contains(search) ||
                           m.PalletCode.Contains(search) || m.PalletName.Contains(search) || m.PalletCondition.Contains(search) ||
                           m.OriginName.Contains(search) || m.DestinationName.Contains(search) || m.PalletOwner.Contains(search) || m.PalletProducer.Contains(search) ||
                           m.RegisteredBy.Contains(search) || m.PalletMovementStatus.Contains(search) || m.LastTransactionName.Contains(search) ||
                           m.LastTransactionCode.Contains(search));

                //columns sorting
                Dictionary<string, Func<VwPalletShipment, object>> cols = new Dictionary<string, Func<VwPalletShipment, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletCode", x => x.PalletCode);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletCondition", x => x.PalletCondition);
                cols.Add("OriginName", x => x.OriginName);
                cols.Add("DestinationName", x => x.DestinationName);
                cols.Add("PalletOwner", x => x.PalletOwner);
                cols.Add("PalletProducer", x => x.PalletProducer);
                cols.Add("ProducedDate", x => x.ProducedDate);
                cols.Add("RegisteredBy", x => x.RegisteredBy);
                cols.Add("RegisteredAt", x => x.RegisteredAt);
                cols.Add("ReceivedBy", x => x.ReceivedBy);
                cols.Add("ReceivedAt", x => x.ReceivedAt);
                cols.Add("PalletMovementStatus", x => x.PalletMovementStatus);
                cols.Add("LastTransactionName", x => x.LastTransactionName);
                cols.Add("LastTransactionCode", x => x.LastTransactionCode);
                cols.Add("LastTransactionDate", x => x.LastTransactionDate);

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

        public int GetDeliveryStock(string OriginId)
        {
            int totalData = 0;
            try
            {
                IQueryable<VwPalletShipment> query = db.VwPalletShipments.AsQueryable().Where(m => m.OriginId.Equals(OriginId));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public IEnumerable<VwPalletShipment> GetIncomingStock(string DestinationId, string search, string sortDirection, string sortColName)
        {
            IQueryable<VwPalletShipment> query = db.VwPalletShipments.AsQueryable().Where(m => m.DestinationId.Equals(DestinationId));
            IEnumerable<VwPalletShipment> list = null;
            try
            {
                query = query
                        .Where(m => m.TagId.Contains(search) ||
                           m.PalletCode.Contains(search) || m.PalletName.Contains(search) || m.PalletCondition.Contains(search) ||
                           m.OriginName.Contains(search) || m.DestinationName.Contains(search) || m.PalletOwner.Contains(search) || m.PalletProducer.Contains(search) ||
                           m.RegisteredBy.Contains(search) || m.PalletMovementStatus.Contains(search) || m.LastTransactionName.Contains(search) ||
                           m.LastTransactionCode.Contains(search));

                //columns sorting
                Dictionary<string, Func<VwPalletShipment, object>> cols = new Dictionary<string, Func<VwPalletShipment, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletCode", x => x.PalletCode);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletCondition", x => x.PalletCondition);
                cols.Add("OriginName", x => x.OriginName);
                cols.Add("DestinationName", x => x.DestinationName);
                cols.Add("PalletOwner", x => x.PalletOwner);
                cols.Add("PalletProducer", x => x.PalletProducer);
                cols.Add("ProducedDate", x => x.ProducedDate);
                cols.Add("RegisteredBy", x => x.RegisteredBy);
                cols.Add("RegisteredAt", x => x.RegisteredAt);
                cols.Add("ReceivedBy", x => x.ReceivedBy);
                cols.Add("ReceivedAt", x => x.ReceivedAt);
                cols.Add("PalletMovementStatus", x => x.PalletMovementStatus);
                cols.Add("LastTransactionName", x => x.LastTransactionName);
                cols.Add("LastTransactionCode", x => x.LastTransactionCode);
                cols.Add("LastTransactionDate", x => x.LastTransactionDate);

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

        public int GetIncomingStock(string DestinationId)
        {
            int totalData = 0;
            try
            {
                IQueryable<VwPalletShipment> query = db.VwPalletShipments.AsQueryable().Where(m => m.OriginId.Equals(DestinationId));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public IEnumerable<VwPalletMovement> GetPalletMovement(string WarehouseId, string startDate, string endDate, string search, string sortDirection, string sortColName)
        {
            IEnumerable<VwPalletMovement> list = Enumerable.Empty<VwPalletMovement>(); ;

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                DateTime StartDate = DateTime.Parse(startDate);
                DateTime EndDate = DateTime.Parse(endDate);
                try
                {
                    IQueryable<VwPalletMovement> query = db.VwPalletMovements.AsQueryable().Where(m => m.WarehouseId.Equals(WarehouseId));
                    query = query.Where(x => DbFunctions.TruncateTime(x.TransactionDate) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(x.TransactionDate) <= DbFunctions.TruncateTime(EndDate));
                    query = query
                            .Where(m => m.TagId.Contains(search) ||
                               m.TagId.Contains(search) || m.PalletName.Contains(search) || m.ScannedBy.Contains(search) ||
                               m.WarehouseName.Contains(search) || m.TransactionCode.Contains(search));

                    //columns sorting
                    Dictionary<string, Func<VwPalletMovement, object>> cols = new Dictionary<string, Func<VwPalletMovement, object>>();
                    cols.Add("TransactionCode", x => x.TransactionCode);
                    cols.Add("WarehouseName", x => x.WarehouseName);
                    cols.Add("TagId", x => x.TagId);
                    cols.Add("PalletName", x => x.PalletName);
                    cols.Add("ScannedDate", x => x.ScannedDate);
                    cols.Add("ScannedBy", x => x.ScannedBy);
                    cols.Add("TransactionDate", x => x.TransactionDate);

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

        public IEnumerable<VwPalletMovement> GetPalletMovement(string WarehouseId)
        {
            IQueryable<VwPalletMovement> query = db.VwPalletMovements.AsQueryable().Where(m => m.WarehouseId.Equals(WarehouseId));
            IEnumerable<VwPalletMovement> list = null;
            try
            {
                //columns sorting
                Dictionary<string, Func<VwPalletMovement, object>> cols = new Dictionary<string, Func<VwPalletMovement, object>>();
                cols.Add("TagId", x => x.TagId);
                //cols.Add("PalletCode", x => x.PalletCode);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("WarehouseName", x => x.WarehouseName);
                cols.Add("ScannedDate", x => x.ScannedDate);
                cols.Add("ScannedBy", x => x.ScannedBy);
                cols.Add("TransactionDate", x => x.TransactionDate);

                list = query.OrderBy(cols["TagId"]);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public int GetTotalPallet()
        {
            int totalData = 0;
            try
            {
                IQueryable<MsPallet> query = db.MsPallets.AsQueryable().Where(m => m.IsDeleted.Equals(false));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public IEnumerable<VwPalletHistory> GetPalletHistory(string WarehouseId, string search, string sortDirection, string sortColName)
        {
            IQueryable<VwPalletHistory> query = db.VwPalletHistories.AsQueryable().Where(m => m.OriginId.Equals(WarehouseId));
            IEnumerable<VwPalletHistory> list = null;
            try
            {

                query = query
                            .Where(m => m.TagId.Contains(search) || m.PalletName.Contains(search) || m.ScannedBy.Contains(search) ||
                               m.WarehouseName.Contains(search) || m.TransactionCode.Contains(search));

                //columns sorting
                Dictionary<string, Func<VwPalletHistory, object>> cols = new Dictionary<string, Func<VwPalletHistory, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletOwner", x => x.PalletOwner);
                cols.Add("PalletProducer", x => x.PalletProducer);
                cols.Add("ProducedDate", x => x.ProducedDate);
                cols.Add("WarehouseCode", x => x.WarehouseCode);
                cols.Add("WarehouseName", x => x.WarehouseName);
                cols.Add("TransactionCode", x => x.TransactionCode);
                cols.Add("TransactionType", x => x.TransactionType);
                cols.Add("TransactionStatus", x => x.TransactionStatus);
                cols.Add("TransactionDate", x => x.TransactionDate);
                cols.Add("ScannedDate", x => x.ScannedDate);
                cols.Add("ScannedBy", x => x.ScannedBy);

                query = query.OrderBy(x => x.TagId).ThenBy(x => x.TransactionDate).ThenBy(x => x.ScannedDate);

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

        public IEnumerable<VwPalletHistory> GetPalletHistory(string WarehouseId)
        {
            IQueryable<VwPalletHistory> query = db.VwPalletHistories.AsQueryable().Where(x => x.OriginId.Equals(WarehouseId));
            IEnumerable<VwPalletHistory> list = null;
            try
            {
                query = query.OrderBy(x => x.TagId).ThenBy(x => x.TransactionDate).ThenBy(x => x.ScannedDate);

                list = query.ToList();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public int GetTotalPalletHistory(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<VwPalletHistory> query = db.VwPalletHistories.AsQueryable().Where(x => x.OriginId.Equals(WarehouseId));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<bool> UpdateManualAsync(string tagId, string newWarehouseName, string palletCondition)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    DateTime currentDate = DateTime.Now;

                    int year = currentDate.Year;
                    int month = currentDate.Month;

                    //get previous
                    MsPallet pallet = await db.MsPallets.FindAsync(tagId);
                    
                    if(pallet != null)
                    {
                      
                        MsWarehouse warehouse = await db.MsWarehouses.Where(m => m.WarehouseName.Equals(newWarehouseName)).FirstOrDefaultAsync();

                        //generate aging row if already settled
                        if (pallet.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString()))
                        {
                            //check previous data, if exist, close unused pallet aging from previous month
                            //update unused
                            //MsPalletAging unusedAging = await db.MsPalletAgings.Where(x => x.PalletId.Equals(item.TagId) && x.AgingType.Equals(Constant.AgingType.UNUSED.ToString()) && x.CurrentMonth.Equals(month) && x.CurrentYear.Equals(year) && x.WarehouseId.Equals(data.WarehouseId)).FirstOrDefaultAsync();
                            MsPalletAging unusedAging = await db.MsPalletAgings.Where(x => x.PalletId.Equals(pallet.TagId) && x.AgingType.Equals(Constant.AgingType.UNUSED.ToString()) && x.WarehouseId.Equals(pallet.WarehouseId)).FirstOrDefaultAsync();
                            //start billing if no row for exact pallet and warehouse
                            if (unusedAging != null && unusedAging.IsActive)
                            {
                                int totalminutes = (int)(currentDate - unusedAging.ReceivedAt).TotalMinutes;
                                unusedAging.TotalMinutes += totalminutes;
                                unusedAging.IsActive = false;
                            }

                            //for origin
                            MsPalletAging aging = await db.MsPalletAgings.Where(x => x.PalletId.Equals(pallet.TagId) && x.AgingType.Equals(Constant.AgingType.USED.ToString()) && x.CurrentMonth.Equals(month) && x.CurrentYear.Equals(year) && x.WarehouseId.Equals(pallet.WarehouseId)).FirstOrDefaultAsync();
                            //start billing if no row for exact pallet and warehouse
                            if (aging != null && aging.IsActive)
                            {
                                int totalminutes = (int)(currentDate - aging.ReceivedAt).TotalMinutes;
                                aging.TotalMinutes += totalminutes;
                                aging.ReceivedAt = currentDate;
                            }

                            //for destination
                            MsPalletAging agingDest = await db.MsPalletAgings.Where(x => x.PalletId.Equals(pallet.TagId) && x.AgingType.Equals(Constant.AgingType.USED.ToString()) && x.CurrentMonth.Equals(month) && x.CurrentYear.Equals(year) && x.WarehouseId.Equals(warehouse.WarehouseId)).FirstOrDefaultAsync();
                            //start billing if no row for exact pallet and warehouse
                            if (agingDest == null)
                            {
                                agingDest = new MsPalletAging
                                {
                                    AgingId = Utilities.CreateGuid("PAG"),
                                    PalletId = pallet.TagId,
                                    WarehouseId = warehouse.WarehouseId,
                                    ReceivedAt = currentDate,
                                    CurrentMonth = month,
                                    CurrentYear = year,
                                    IsActive = true,
                                    AgingType = Constant.AgingType.USED.ToString()
                                };

                                db.MsPalletAgings.Add(agingDest);
                            }
                            else
                            {
                                if (agingDest.IsActive)
                                {
                                    //update received date
                                    agingDest.ReceivedAt = currentDate;
                                }

                            }
                        }

                        pallet.WarehouseId = warehouse.WarehouseId;
                        pallet.WarehouseCode = warehouse.WarehouseCode;
                        pallet.WarehouseName = warehouse.WarehouseName;

                        pallet.PalletCondition = palletCondition;

                        //update pallet new warehouse


                        await db.SaveChangesAsync();
                        transaction.Commit();
                        status = true;

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
    }
}