using Facelift_App.Helper;
using Facelift_App.Models.Api;
using Facelift_App.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Facelift_App.Controllers.Api
{
    public class ShipmentManualController : ApiController
    {
        private readonly IShipments IShipments;
        private readonly IWarehouses IWarehouses;
        private readonly IPallets IPallets;
        private readonly ITransporters ITransporters;
        private readonly ITransporterDrivers IDrivers;
        private readonly ITransporterTrucks ITrucks;

        public ShipmentManualController(IShipments Shipments, IWarehouses Warehouses,
            IPallets Pallets, ITransporters Transporters, ITransporterDrivers Drivers,
            ITransporterTrucks Trucks)
        {
            IShipments = Shipments;
            IWarehouses = Warehouses;
            IPallets = Pallets;
            ITransporters = Transporters;
            IDrivers = Drivers;
            ITrucks = Trucks;
        }

        public async Task<IHttpActionResult> Post(ShipmentVM dataVM)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "Invalid form submission.";
            bool status = false;
            try
            {
                MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(dataVM.OriginId);
                MsWarehouse destination = await IWarehouses.GetDataByIdAsync(dataVM.DestinationId);
                MsTransporter transporter = await ITransporters.GetDataByIdAsync(dataVM.TransporterId);
                MsDriver driver = await IDrivers.GetDataByIdAsync(dataVM.DriverId);
                MsTruck truck = await ITrucks.GetDataByIdAsync(dataVM.TruckId);


                TrxShipmentHeader data = new TrxShipmentHeader
                {
                    TransactionId = Utilities.CreateGuid("SHP"),
                    ShipmentNumber = dataVM.ShipmentNumber,
                    Remarks = dataVM.Remarks,
                    WarehouseId = dataVM.OriginId,
                    WarehouseCode = warehouse.WarehouseCode,
                    WarehouseName = warehouse.WarehouseName,
                    DestinationId = dataVM.DestinationId,
                    DestinationCode = destination.WarehouseCode,
                    DestinationName = destination.WarehouseName,
                    TransporterId = dataVM.TransporterId,
                    TransporterName = transporter.TransporterName,
                    DriverId = dataVM.DriverId,
                    DriverName = driver.DriverName,
                    TruckId = dataVM.TruckId,
                    PlateNumber = truck.PlateNumber,
                    PalletQty = dataVM.PalletQty,
                    TransactionStatus = Constant.TransactionStatus.CLOSED.ToString(),
                    ShipmentStatus = Constant.ShipmentStatus.DISPATCH.ToString(),
                    CreatedBy = "System",
                    CreatedAt = Convert.ToDateTime(dataVM.TransactionDate)
                };

                foreach (ShipmentItemVM outboundItem in dataVM.items)
                {
                    string tag = outboundItem.TagId;
                    string tagId = Utilities.ConvertTag(tag);
                    MsPallet pallet = await IPallets.GetDataByTagWarehouseIdAsync(tagId, data.WarehouseId);
                    if (pallet != null && pallet.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())
                        && pallet.PalletCondition.Equals(Constant.PalletCondition.GOOD.ToString()))
                    {
                        TrxShipmentItem item = await IShipments.GetDataByTransactionTagIdAsync(data.TransactionId, tagId);
                        if (item == null)
                        {
                            item = new TrxShipmentItem();
                            item.TransactionItemId = Utilities.CreateGuid("SHI");
                            item.TransactionId = data.TransactionId;
                            item.TagId = tagId;
                            item.ScannedBy = "System";
                            item.ScannedAt = Convert.ToDateTime(outboundItem.ScannedAt);
                            item.DispatchedBy = "System";
                            item.DispatchedAt = Convert.ToDateTime(outboundItem.ScannedAt);
                            item.PalletMovementStatus = Constant.PalletMovementStatus.OT.ToString();
                            data.TrxShipmentItems.Add(item);
                        }
                    }

                }

                status = await IShipments.DispatchManualAsync(data);
                if (status)
                {
                    message = "Create data succeeded.";
                }
                else
                {
                    message = "Create data failed. Please contact system administrator.";
                }

            }
            catch (HttpRequestException reqpEx)
            {
                message = reqpEx.Message;
                return BadRequest();
            }
            catch (HttpResponseException respEx)
            {
                message = respEx.Message;
                return NotFound();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                //return InternalServerError();
            }


            obj.Add("status", status);
            obj.Add("message", message);



            return Ok(obj);
        }


        public async Task<IHttpActionResult> Put(ShipmentVM dataVM)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "Invalid form submission.";
            bool status = false;
            try
            {
                MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(dataVM.OriginId);
                MsWarehouse destination = await IWarehouses.GetDataByIdAsync(dataVM.DestinationId);
                MsTransporter transporter = await ITransporters.GetDataByIdAsync(dataVM.TransporterId);
                MsDriver driver = await IDrivers.GetDataByIdAsync(dataVM.DriverId);
                MsTruck truck = await ITrucks.GetDataByIdAsync(dataVM.TruckId);


                TrxShipmentHeader data = new TrxShipmentHeader
                {
                    TransactionId = Utilities.CreateGuid("SHP"),
                    ShipmentNumber = dataVM.ShipmentNumber,
                    Remarks = dataVM.Remarks,
                    WarehouseId = dataVM.OriginId,
                    WarehouseCode = warehouse.WarehouseCode,
                    WarehouseName = warehouse.WarehouseName,
                    DestinationId = dataVM.DestinationId,
                    DestinationCode = destination.WarehouseCode,
                    DestinationName = destination.WarehouseName,
                    TransporterId = dataVM.TransporterId,
                    TransporterName = transporter.TransporterName,
                    DriverId = dataVM.DriverId,
                    DriverName = driver.DriverName,
                    TruckId = dataVM.TruckId,
                    PlateNumber = truck.PlateNumber,
                    PalletQty = dataVM.PalletQty,
                    TransactionStatus = Constant.TransactionStatus.CLOSED.ToString(),
                    ShipmentStatus = Constant.ShipmentStatus.DISPATCH.ToString(),
                    CreatedBy = "System",
                    CreatedAt = Convert.ToDateTime(dataVM.TransactionDate)
                };

                foreach (ShipmentItemVM outboundItem in dataVM.items)
                {
                    string tag = outboundItem.TagId;
                    string tagId = Utilities.ConvertTag(tag);
                    MsPallet pallet = await IPallets.GetDataByTagWarehouseIdAsync(tagId, data.WarehouseId);
                    if (pallet != null && pallet.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())
                        && pallet.PalletCondition.Equals(Constant.PalletCondition.GOOD.ToString()))
                    {
                        TrxShipmentItem item = await IShipments.GetDataByTransactionTagIdAsync(data.TransactionId, tagId);
                        if (item == null)
                        {
                            item = new TrxShipmentItem();
                            item.TransactionItemId = Utilities.CreateGuid("SHI");
                            item.TransactionId = data.TransactionId;
                            item.TagId = tagId;
                            item.ScannedBy = "System";
                            item.ScannedAt = Convert.ToDateTime(outboundItem.ScannedAt);
                            item.DispatchedBy = "System";
                            item.DispatchedAt = Convert.ToDateTime(outboundItem.ScannedAt);
                            item.PalletMovementStatus = Constant.PalletMovementStatus.OT.ToString();
                            data.TrxShipmentItems.Add(item);
                        }
                    }

                }

                status = await IShipments.ReceiveManualAsync(data);
                if (status)
                {
                    message = "Update data succeeded.";
                }
                else
                {
                    message = "Update data failed. Please contact system administrator.";
                }

            }
            catch (HttpRequestException reqpEx)
            {
                message = reqpEx.Message;
                return BadRequest();
            }
            catch (HttpResponseException respEx)
            {
                message = respEx.Message;
                return NotFound();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                //return InternalServerError();
            }


            obj.Add("status", status);
            obj.Add("message", message);



            return Ok(obj);
        }

    }
}
