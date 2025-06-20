using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Facelift_App.Repositories
{
    public class DashboardRepository : IDashboards
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public int TotalCycleCount(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxCycleCountHeader> query = db.TrxCycleCountHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && !x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()) && x.IsDeleted == false);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public int TotalInbound(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable().Where(x => x.DestinationId.Equals(WarehouseId) && x.ShipmentStatus.Equals(Constant.ShipmentStatus.DISPATCH.ToString()) && x.IsDeleted == false);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public int TotalInspection(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxInspectionHeader> query = db.TrxInspectionHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && !x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()) && x.IsDeleted == false);
                //IQueryable<TrxAccidentHeader> query = db.TrxAccidentHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && !x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()) && x.IsDeleted == false);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public int TotalOutbound(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.ShipmentStatus.Equals(Constant.ShipmentStatus.LOADING.ToString()) && x.IsDeleted == false);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public int TotalRegistration(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxRegistrationHeader> query = db.TrxRegistrationHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && !x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()) && x.IsDeleted == false);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public int TotalOutboundLoading(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable()
                    .Where(x => x.WarehouseId.Equals(WarehouseId) 
                    && x.ShipmentStatus.Equals(Constant.ShipmentStatus.LOADING.ToString()) 
                    && x.IsDeleted == false);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public int TotalOutboundTransit(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable()
                    .Where(x => x.WarehouseId.Equals(WarehouseId)
                    && x.ShipmentStatus.Equals(Constant.ShipmentStatus.DISPATCH.ToString())
                    && x.IsDeleted == false);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public int TotalOutboundFinished(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable()
                    .Where(x => x.WarehouseId.Equals(WarehouseId)
                    && x.ShipmentStatus.Equals(Constant.ShipmentStatus.RECEIVE.ToString())
                    && x.IsDeleted == false);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public int TotalInboundTransit(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable()
                    .Where(x => x.DestinationId.Equals(WarehouseId)
                    && x.ShipmentStatus.Equals(Constant.ShipmentStatus.DISPATCH.ToString())
                    && x.IsDeleted == false);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public int TotalInboundFinished(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable()
                    .Where(x => x.DestinationId.Equals(WarehouseId)
                    && x.ShipmentStatus.Equals(Constant.ShipmentStatus.RECEIVE.ToString())
                    && x.IsDeleted == false);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public int TotalPalletOutbound(string OriginId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxShipmentItem> query = db.TrxShipmentItems.AsQueryable()
                    .Where(x => x.TrxShipmentHeader.WarehouseId.Equals(OriginId)
                    && x.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString())
                    && x.TrxShipmentHeader.ShipmentStatus.Equals(Constant.ShipmentStatus.DISPATCH.ToString())
                    && x.TrxShipmentHeader.IsDeleted == false);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public int TotalPalletInbound(string DestinationId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxShipmentItem> query = db.TrxShipmentItems.AsQueryable()
                    .Where(x => x.TrxShipmentHeader.DestinationId.Equals(DestinationId)
                    && x.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString())
                    && x.TrxShipmentHeader.ShipmentStatus.Equals(Constant.ShipmentStatus.DISPATCH.ToString())
                    && x.TrxShipmentHeader.IsDeleted == false);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public IEnumerable<PalletStockDTO> PalletDelivery(string OriginId)
        {
            IEnumerable<PalletStockDTO> listShipment = Enumerable.Empty<PalletStockDTO>();
            try
            {
                var query = db.TrxShipmentItems
                  .Where(m => m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString()))
                  .Where(m => m.TrxShipmentHeader.WarehouseId.Equals(OriginId))
                  .Where(m => m.TrxShipmentHeader.TransactionStatus.Equals(Constant.TransactionStatus.PROGRESS.ToString()))
                  .Where(m => m.TrxShipmentHeader.ShipmentStatus.Equals(Constant.ShipmentStatus.DISPATCH.ToString()))
                  .Where(m => m.TrxShipmentHeader.IsDeleted == false)
                  .GroupBy(g => new { Name = g.TrxShipmentHeader.DestinationName})
                  .Select(g => new {  DestinationName = g.Key.Name, Count = g.Count() }).ToList();

                List<PalletStockDTO> list = new List<PalletStockDTO>();
                foreach (var item in query)
                {
                    PalletStockDTO palletStockDTO = new PalletStockDTO
                    {
                        WarehouseName = item.DestinationName,
                        TotalPallet = item.Count.ToString()
                    };
                    list.Add(palletStockDTO);
                }

                listShipment = list;
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return listShipment;
        }

        public IEnumerable<PalletStockDTO> PalletIncoming(string DestinationId)
        {
            IEnumerable<PalletStockDTO> listShipment = Enumerable.Empty<PalletStockDTO>();
            try
            {
                var query = db.TrxShipmentItems
                  .Where(m => m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString()))
                  .Where(m => m.TrxShipmentHeader.DestinationId.Equals(DestinationId))
                  .Where(m => m.TrxShipmentHeader.TransactionStatus.Equals(Constant.TransactionStatus.PROGRESS.ToString()))
                  .Where(m => m.TrxShipmentHeader.ShipmentStatus.Equals(Constant.ShipmentStatus.DISPATCH.ToString()))
                  .Where(m => m.TrxShipmentHeader.IsDeleted == false)
                  .GroupBy(g => new { Name = g.TrxShipmentHeader.WarehouseName })
                  .Select(g => new { WarehouseName = g.Key.Name, Count = g.Count() }).ToList();

                List<PalletStockDTO> list = new List<PalletStockDTO>();
                foreach (var item in query)
                {
                    PalletStockDTO palletStockDTO = new PalletStockDTO
                    {
                        WarehouseName = item.WarehouseName,
                        TotalPallet = item.Count.ToString()
                    };
                    list.Add(palletStockDTO);
                }

                listShipment = list;
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return listShipment;
        }

       

        public IEnumerable<VwShipmentAccident> GetListAging(string WarehouseId)
        {
            IQueryable<VwShipmentAccident> query = db.VwShipmentAccidents.AsQueryable()
              .Where(x => x.ShipmentWarehouseId.Equals(WarehouseId) && x.AccidentTransactionStatus.Equals(Constant.TransactionStatus.PROGRESS.ToString())
              && x.AccidentIsDeleted == false);
            IEnumerable<VwShipmentAccident> list = null;
            try
            {
                list = query.OrderByDescending(x => x.AccidentTransactionCode);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public IEnumerable<ShipmentAgingDTO> GetShipmentAging(string OriginId)
        {
            IEnumerable<ShipmentAgingDTO> listShipment = Enumerable.Empty<ShipmentAgingDTO>();
            try
            {
                var query = db.VwAgingShipments
                  .Where(m => m.OriginId.Equals(OriginId)).ToList();

                List<ShipmentAgingDTO> list = new List<ShipmentAgingDTO>();
                foreach (var item in query)
                {
                    ShipmentAgingDTO shipmentAging = new ShipmentAgingDTO()
                    {
                        DeliveryNumber = item.DeliveryNumber,
                        OriginCode = item.OriginCode,
                        OriginName = item.OriginName,
                        DestinationCode = item.DestinationCode,
                        DestinationName = item.DestinationName,
                        ShipmentStatus = item.ShipmentStatus,
                        CreatedAt = Utilities.NullDateTimeToString(item.CreatedAt),
                        AgingDay = item.AgingDay.ToString(),
                        AgingMin = item.AgingMin.ToString()
                    };
                    list.Add(shipmentAging);
                }

                listShipment = list;
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return listShipment;
        }

        public int TotalRecall(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxRecallHeader> query = db.TrxRecallHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && !x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()) && x.IsDeleted == false);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }
    }
}