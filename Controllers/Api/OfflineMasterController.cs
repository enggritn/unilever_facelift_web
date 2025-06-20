using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Models.Api;
using Facelift_App.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Facelift_App.Controllers.Api
{
    public class OfflineMasterController : ApiController
    {
        FaceliftEntities db = new FaceliftEntities();
      
        public IHttpActionResult Get()
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "";
            bool status = false;
            IEnumerable<OfflineWarehouseDTO> warehouses = null;
            IEnumerable<OfflineTransporterDTO> transporters = null;
            IEnumerable<OfflineDriverDTO> drivers = null;
            IEnumerable<OfflineTruckDTO> trucks = null;
            try
            {
                IEnumerable<MsWarehouse> listWarehouse = db.MsWarehouses.ToList();

                warehouses = from m in listWarehouse
                             select new OfflineWarehouseDTO
                             {
                                 WarehouseCode = m.WarehouseCode,
                                 WarehouseName = m.WarehouseAlias
                             };

                IEnumerable<MsTransporter> listTransporter = db.MsTransporters.ToList();


                transporters = from m in listTransporter
                               select new OfflineTransporterDTO
                               {
                                 TransporterId = m.TransporterId,
                                 TransporterName = m.TransporterName
                             };

                IEnumerable<MsDriver> listDriver = db.MsDrivers.ToList();

                drivers = from m in listDriver
                          select new OfflineDriverDTO
                             {
                              TransporterId = m.TransporterId,
                                 DriverId = m.DriverId,
                                 DriverName = m.DriverName
                             };

                IEnumerable<MsTruck> listTruck = db.MsTrucks.ToList();

                trucks = from m in listTruck
                             select new OfflineTruckDTO
                             {
                                 TransporterId = m.TransporterId,
                                 TruckId = m.TruckId,
                                 PlateNumber = m.PlateNumber
                             };


                status = true;
                message = "Data found.";
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
            obj.Add("warehouses", warehouses);
            obj.Add("transporters", transporters);
            obj.Add("drivers", drivers);
            obj.Add("trucks", trucks);


            return Ok(obj);
        }

    }

}
