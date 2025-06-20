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
    public class OfflineShipmentController : ApiController
    {
        FaceliftEntities db = new FaceliftEntities();
      
        public IHttpActionResult Get()
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "";
            bool status = false;
            IEnumerable<OfflineShipmentHeaderDTO> headers = null;
            IEnumerable<LogShipmentOfflineHeader> list = null;
            try
            {
                list = db.LogShipmentOfflineHeaders.ToList();

                
                headers = from x in list
                    select new OfflineShipmentHeaderDTO
                    {
                        TransactionCode = x.TransactionCode,
                        ShipmentNumber = !string.IsNullOrEmpty(x.ShipmentNumber) ? x.ShipmentNumber : "-",
                        OriginName = x.WarehouseName,
                        DestinationName = x.DestinationName,
                        TransporterName = x.TransporterName,
                        DriverName = x.DriverName,
                        PlateNumber = x.PlateNumber,
                        PalletQty = Utilities.FormatThousand(x.PalletQty),
                        Remarks = !string.IsNullOrEmpty(x.Remarks) ? x.Remarks : "-",
                        CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
                        UploadedAt = Utilities.NullDateTimeToString(x.CreatedAt)

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
            obj.Add("data", headers);

            return Ok(obj);
        }

        public async Task<IHttpActionResult> GetById(string TransactionCode)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "";
            bool status = false;
            OfflineShipmentHeaderDTO header = null;
            try
            {
                LogShipmentOfflineHeader data = db.LogShipmentOfflineHeaders.Where(m => m.TransactionCode.Equals(TransactionCode)).FirstOrDefault();
                if (data == null)
                {
                    throw new Exception("Transaction not recognized.");
                }

                header = new OfflineShipmentHeaderDTO
                {
                    TransactionCode = data.TransactionCode,
                    ShipmentNumber = !string.IsNullOrEmpty(data.ShipmentNumber) ? data.ShipmentNumber : "-",
                    OriginName = data.WarehouseName,
                    DestinationName = data.DestinationName,
                    TransporterName = data.TransporterName,
                    DriverName = data.DriverName,
                    PlateNumber = data.PlateNumber,
                    PalletQty = Utilities.FormatThousand(data.PalletQty),
                    Remarks = !string.IsNullOrEmpty(data.Remarks) ? data.Remarks : "-",
                    CreatedAt = Utilities.NullDateTimeToString(data.CreatedAt),
                    UploadedAt = Utilities.NullDateTimeToString(data.CreatedAt)
                };

                IEnumerable<LogShipmentOfflineItem> items = db.LogShipmentOfflineItems.Where(m => m.TransactionCode.Equals(header.TransactionCode)).ToList();
                header.items = from x in items
                                     select new OfflineShipmentItemDTO
                                     {
                                         TransactionCode = x.TransactionCode,
                                         TagId = x.TagId,
                                         ScannedAt = Utilities.NullDateTimeToString(x.ScannedAt),
                                         UploadedAt = Utilities.NullDateTimeToString(x.UploadedAt)
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
            obj.Add("data", header);

            return Ok(obj);
        }


        public async Task<IHttpActionResult> Post(List<OfflineHeaderShipmentVM> listShipment)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "Invalid form submission.";
            bool status = false;
            var re = Request;
            var headers = re.Headers;
            try
            {
                //get user access
                if (headers.Contains(Constant.facelift_token_name))
                {
                    string token = headers.GetValues(Constant.facelift_token_name).First();
                    if (token.Equals(Constant.facelift_token_key))
                    {
                        DateTime uploadDate = DateTime.Now;

                        foreach (OfflineHeaderShipmentVM shipment in listShipment)
                        {
                            LogShipmentOfflineHeader header = db.LogShipmentOfflineHeaders.Where(m => m.TransactionCode.Equals(shipment.TransactionCode)).FirstOrDefault();
                            if (header == null)
                            {
                                header = new LogShipmentOfflineHeader();
                                header.TransactionCode = shipment.TransactionCode;
                                header.ShipmentNumber = shipment.ShipmentNumber;
                                header.WarehouseName = shipment.OriginName;
                                header.DestinationName = shipment.DestinationName;
                                header.TransporterName = shipment.TransporterName;
                                header.DriverName = shipment.DriverName;
                                header.PlateNumber = shipment.PlateNumber;
                                header.PalletQty = shipment.PalletQty;
                                header.CreatedAt = DateTime.ParseExact(shipment.TransactionDate, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                                header.UploadedAt = uploadDate;
                                header.Remarks = shipment.Remarks;


                                db.LogShipmentOfflineHeaders.Add(header);
                            }
                        }

                        await db.SaveChangesAsync();

                        status = true;
                        message = "Sync succeeded.";
                    }
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

        public async Task<IHttpActionResult> Put(List<OfflineItemShipmentVM> listShipment)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "Invalid form submission.";
            bool status = false;
            var re = Request;
            var headers = re.Headers;
            try
            {
                //get user access
                if (headers.Contains(Constant.facelift_token_name))
                {
                    string token = headers.GetValues(Constant.facelift_token_name).First();
                    string WarehouseName = headers.GetValues("warehouse_name").First();
                    if (token.Equals(Constant.facelift_token_key))
                    {
                        DateTime uploadDate = DateTime.Now;
                        foreach (OfflineItemShipmentVM shipment in listShipment)
                        {
                            LogShipmentOfflineItem item = db.LogShipmentOfflineItems.Where(m => m.TransactionCode.Equals(shipment.TransactionCode) &&m.TagId.Equals(shipment.TagId)).FirstOrDefault();
                            if (item == null)
                            {
                                item = new LogShipmentOfflineItem();
                                item.TagId = shipment.TagId;
                                item.TransactionCode = shipment.TransactionCode;
                                item.ScannedAt = Convert.ToDateTime(shipment.ScannedAt);
                                item.UploadedAt = uploadDate;
                                item.TransactionItemId = Utilities.CreateGuid("OFF");
                                item.ShipmentType = shipment.ShipmentType;
                                item.WarehouseName = WarehouseName;

                                db.LogShipmentOfflineItems.Add(item);
                            }
                        }

                        await db.SaveChangesAsync();

                        status = true;
                        message = "Sync succeeded.";
                    }
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
