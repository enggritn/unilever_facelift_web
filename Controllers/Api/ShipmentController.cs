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
    public class ShipmentController : ApiController
    {
        private readonly IUsers IUsers;
        private readonly IMenus IMenus;
        private readonly IShipments IShipments;
        private readonly IWarehouses IWarehouses;
        private readonly IPallets IPallets;

        public ShipmentController(IUsers Users, IMenus Menus, IShipments Shipments, IWarehouses Warehouses, IPallets Pallets)
        {
            IUsers = Users;
            IMenus = Menus;
            IShipments = Shipments;
            IWarehouses = Warehouses;
            IPallets = Pallets;
        }

        public IHttpActionResult Get()
        {
            return Ok();
        }

        public async Task<IHttpActionResult> GetByTransactionCode(string TransactionCode)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "";
            bool status = false;
            var re = Request;
            var headers = re.Headers;
            string TransactionId = "";
            string ShipmentType = "";
            try
            {
                //get user access
                if (headers.Contains("warehouseName"))
                {
                    string warehouseName = headers.GetValues("warehouseName").First();
                    MsWarehouse warehouse = await IWarehouses.GetDataByWarehouseAliasAsync(warehouseName);
                    if (warehouse != null)
                    {
                        TrxShipmentHeader header = await IShipments.GetDataByTransactionCodeAsync(TransactionCode);
                        if (header == null)
                        {
                            throw new Exception("Transaction not recognized.");
                        }

                        if (header.IsDeleted)
                        {
                            throw new Exception("Transaction already deleted.");
                        }

                        if (header.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                        {
                            throw new Exception("Transaction already closed.");
                        }

                        //check if origin (outbound) or destination (inbound)
                        if (header.WarehouseId.Equals(warehouse.WarehouseId))
                        {
                            if (!header.ShipmentStatus.Equals(Constant.ShipmentStatus.LOADING.ToString())){
                                throw new Exception("Delivery note is not valid.");
                            }
                            //outbound
                            status = true;
                            message = "Delivery note is valid.";
                            TransactionId = header.TransactionId;
                            ShipmentType = "OUTBOUND";
                        }
                        else if (header.DestinationId.Equals(warehouse.WarehouseId))
                        {
                            if (!header.ShipmentStatus.Equals(Constant.ShipmentStatus.DISPATCH.ToString())){
                                throw new Exception("Delivery note is not valid.");
                            }
                            //inbound
                            status = true;
                            message = "Delivery note is valid.";
                            TransactionId = header.TransactionId;
                            ShipmentType = "INBOUND";
                            obj.Add("TotalPallet", header.TrxShipmentItems.Where(m => m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString())).Count());
                            //add total scanned pallet object
                        }
                        else
                        {
                            throw new Exception("Unrecognized warehouse for current transaction.");
                        }                        
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return Unauthorized();
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
            obj.Add("TransactionId", TransactionId);
            obj.Add("ShipmentType", ShipmentType);

            return Ok(obj);
        }

        public async Task<IHttpActionResult> Post(Shipment shipment)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "";
            bool status = false;
            var re = Request;
            var headers = re.Headers;
            string TransactionId = "";
            int totalUnscanned = 0;
            int totalQtyShipment = 0;

            try
            {
                //get user access
                if (headers.Contains("warehouseName"))
                {
                    string warehouseName = headers.GetValues("warehouseName").First();
                    MsWarehouse warehouse = await IWarehouses.GetDataByWarehouseAliasAsync(warehouseName);
                    if (warehouse != null)
                    {
                        TrxShipmentHeader header = await IShipments.GetDataByIdAsync(shipment.TransactionId);
                        if (header == null)
                        {
                            throw new Exception("Transaction not recognized.");
                        }

                        if (header.IsDeleted)
                        {
                            throw new Exception("Transaction already deleted.");
                        }

                        if (header.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                        {
                            throw new Exception("Transaction already closed.");
                        }

                        string username = "System";
                        totalQtyShipment = header.PalletQty;

                        //check if origin (outbound) or destination (inbound)
                        if (header.WarehouseId.Equals(warehouse.WarehouseId))
                        {
                            if (!header.ShipmentStatus.Equals(Constant.ShipmentStatus.LOADING.ToString()))
                            {
                                throw new Exception("Delivery note is not valid.");
                            }

                            List<TrxShipmentItem> detail = header.TrxShipmentItems.ToList();
                            //outbound
                            DateTime currentDate = DateTime.Now;
                            int count = 0;
                            foreach (string tag in shipment.items)
                            {
                                string tagId = Utilities.ConvertTag(tag);

                                if (count < totalQtyShipment)
                                {
                                    MsPallet cek_pallet = await IPallets.GetDataByTagIdAsync(tagId);
                                    if (cek_pallet != null && !cek_pallet.WarehouseId.Equals(header.WarehouseId) && !cek_pallet.PalletCondition.Equals(Constant.PalletMovementStatus.OT.ToString()))
                                    {
                                        status = await IShipments.UpdateItemAsync(header.WarehouseId, tagId);
                                    }
                                    else if (cek_pallet != null && cek_pallet.PalletCondition.Equals(Constant.PalletCondition.FREEZE.ToString()))
                                    {
                                        status = await IShipments.UpdateItemAsync(header.WarehouseId, tagId);
                                    }

                                    TrxShipmentItem item = await IShipments.GetDataByTransactionTagIdAsync(header.TransactionId, tagId);
                                    if (item == null)
                                    {
                                        TrxShipmentItem item2 = header.TrxShipmentItems.Where(m => m.TagId.Equals(tagId)).FirstOrDefault();
                                        if (item2 == null)
                                        {
                                            item = new TrxShipmentItem();
                                            item.TransactionItemId = Utilities.CreateGuid("SHI");
                                            item.TransactionId = header.TransactionId;
                                            item.TagId = tagId;
                                            item.ScannedBy = username;
                                            item.ScannedAt = currentDate;
                                            item.DispatchedBy = username;
                                            item.DispatchedAt = currentDate;
                                            item.PalletMovementStatus = Constant.PalletMovementStatus.OT.ToString();

                                            MsPallet pallet = await IPallets.GetDataByTagWarehouseIdAsync(tagId, header.WarehouseId);
                                            if (pallet != null
                                                && pallet.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())
                                                && pallet.PalletCondition.Equals(Constant.PalletCondition.GOOD.ToString()))
                                            {
                                                //update pallet status in pallet stock, stock validation
                                                header.TrxShipmentItems.Add(item);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //update
                                        item.DispatchedBy = username;
                                        item.DispatchedAt = currentDate;
                                        item.PalletMovementStatus = Constant.PalletMovementStatus.OT.ToString();
                                        //update current data index
                                        int index = detail.IndexOf(item);
                                        detail[index] = item;
                                    }

                                    if (cek_pallet != null)
                                    {
                                        // insert data temp outbound
                                        TrxShipmentItemTemp itemtemp = await IShipments.GetDataByTransactionTagIdTempAsync(header.TransactionId, tagId, "OUTBOUND");
                                        if (itemtemp == null)
                                        {
                                            itemtemp = new TrxShipmentItemTemp();
                                            itemtemp.TempID = Utilities.CreateGuid("SHI");
                                            itemtemp.TransactionId = header.TransactionId;
                                            itemtemp.TagId = tagId;
                                            itemtemp.ScannedBy = username;
                                            itemtemp.ScannedAt = currentDate;
                                            itemtemp.StatusShipment = "OUTBOUND";

                                            await IShipments.InsertItemTempAsync(itemtemp);
                                        }
                                    }
                                }                                
                                count++;
                            }

                            if (!header.PalletQty.Equals(header.TrxShipmentItems.Count()))
                            {
                                int warningLimit = Convert.ToInt32(ConfigurationManager.AppSettings["warning_limit"].ToString());
                                header.WarningCount += 1;
                                if (header.WarningCount == warningLimit)
                                {
                                    //send email info
                                    List<string> emails = new List<string>();
                                    MsUser pic1 = await IUsers.GetDataByUsernameAsync(warehouse.PIC1);
                                    MsUser pic2 = await IUsers.GetDataByUsernameAsync(warehouse.PIC2);
                                    if (pic1 != null)
                                    {
                                        emails.Add(pic1.UserEmail);
                                    }

                                    if (pic2 != null)
                                    {
                                        emails.Add(pic2.UserEmail);
                                    }

                                    ShipmentDTO shipmentData = new ShipmentDTO();
                                    shipmentData.TransactionCode = header.TransactionCode;
                                    shipmentData.PalletQty = Utilities.FormatThousand(header.PalletQty);
                                    shipmentData.CreatedAt = Utilities.NullDateToString(header.CreatedAt);
                                    shipmentData.WarehouseName = header.WarehouseName;
                                    shipmentData.DestinationName = header.DestinationName;
                                    shipmentData.TransporterName = header.TransporterName;
                                    shipmentData.DriverName = header.DriverName;
                                    shipmentData.PlateNumber = header.PlateNumber;
                                    String body = RenderViewToString("Outbound", "WarningEmail", shipmentData);

                                    Mailing mailing = new Mailing();
                                    Task.Factory.StartNew(() => mailing.SendEmail(emails, "Facelift - Outbound Warning", body));
                                    header.WarningCount = 0;
                                    await IShipments.UpdateWarningCountAsync(header);
                                }
                                
                                throw new Exception(string.Format("Pallet Quantity must be equal to {0}.", header.PalletQty));
                            }
                            //if 1 or more item uploaded, transaction status will be changed as progress
                            if (header.TrxShipmentItems != null && header.TrxShipmentItems.Count() > 0)
                            {
                                header.TransactionStatus = Constant.TransactionStatus.PROGRESS.ToString();
                                header.ShipmentStatus = Constant.ShipmentStatus.DISPATCH.ToString();
                            }
                            else
                            {
                                throw new Exception("Pallet dispatch failed, no pallet was scanned.");
                            }
                            //reset warning count
                            header.WarningCount = 0;
                            await IShipments.UpdateWarningCountAsync(header);
                            status = await IShipments.InsertItemAsync(header, username, "Dispatch Item (RFID Gate)");
                            if (status)
                            {
                                message = "Pallet dispatch successfuly.";
                            }
                            else
                            {
                                message = "Pallet dispatch failed. Please contact system administrator.";
                            }
                        }
                        else if (header.DestinationId.Equals(warehouse.WarehouseId))
                        {
                            if (!header.ShipmentStatus.Equals(Constant.ShipmentStatus.DISPATCH.ToString()))
                            {
                                throw new Exception("Delivery note is not valid.");
                            }

                            DateTime currentDate = DateTime.Now;
                            //inbound
                            List<TrxShipmentItem> detail = header.TrxShipmentItems.ToList();
                            foreach (string tag in shipment.items)
                            {
                                string tagId = Utilities.ConvertTag(tag);
                                TrxShipmentItem item = detail.Where(m => m.TagId.Equals(tagId)).FirstOrDefault();
                                if (item != null)
                                {
                                    if (item.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString()))
                                    {
                                        item.ReceivedBy = username;
                                        item.ReceivedAt = currentDate;
                                        item.PalletMovementStatus = Constant.PalletMovementStatus.IN.ToString();
                                        //update current data index
                                        int index = detail.IndexOf(item);
                                        detail[index] = item;
                                    }
                                }

                                MsPallet cek_pallet = await IPallets.GetDataByTagIdAsync(tagId);
                                if (cek_pallet != null)
                                {
                                    // insert data temp inbound
                                    TrxShipmentItemTemp itemtemp = await IShipments.GetDataByTransactionTagIdTempAsync(header.TransactionId, tagId, "INBOUND");
                                    if (itemtemp == null)
                                    {
                                        itemtemp = new TrxShipmentItemTemp();
                                        itemtemp.TempID = Utilities.CreateGuid("SHI");
                                        itemtemp.TransactionId = header.TransactionId;
                                        itemtemp.TagId = tagId;
                                        itemtemp.ScannedBy = username;
                                        itemtemp.ScannedAt = currentDate;
                                        itemtemp.StatusShipment = "INBOUND";

                                        await IShipments.InsertItemTempAsync(itemtemp);
                                    }
                                }
                            }

                            int totalRow = detail.Count();
                            int totalScanned = detail.Where(m => m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.IN.ToString()) || m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())).Count();
                            totalUnscanned = detail.Where(m => m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString())).Count();

                            //auto close here
                            if (totalRow == totalScanned)
                            {
                                header.TransactionStatus = Constant.TransactionStatus.CLOSED.ToString();
                                header.ShipmentStatus = Constant.ShipmentStatus.RECEIVE.ToString();

                                // delete data temp by transaction id trxshipmentheader
                                TrxShipmentItemTemp itemp = await IShipments.GetDataByTransactionIdTempAsync(header.TransactionId);
                                if (itemp != null)
                                {
                                    bool delete = await IShipments.DeleteItemTempAsync(itemp);
                                }
                            }

                            if (totalRow != totalScanned)
                            {
                                int warningLimit = Convert.ToInt32(ConfigurationManager.AppSettings["warning_limit"].ToString());
                                header.WarningCount += 1;
                                if (header.WarningCount == warningLimit)
                                {
                                    //send email info
                                    List<string> emails = new List<string>();
                                    MsUser pic1 = await IUsers.GetDataByUsernameAsync(warehouse.PIC1);
                                    MsUser pic2 = await IUsers.GetDataByUsernameAsync(warehouse.PIC2);
                                    if (pic1 != null)
                                    {
                                        emails.Add(pic1.UserEmail);
                                    }

                                    if (pic2 != null)
                                    {
                                        emails.Add(pic2.UserEmail);
                                    }

                                    ShipmentDTO shipmentData = new ShipmentDTO();
                                    shipmentData.TransactionCode = header.TransactionCode;
                                    shipmentData.PalletQty = Utilities.FormatThousand(header.PalletQty);
                                    shipmentData.CreatedAt = Utilities.NullDateToString(header.CreatedAt);
                                    shipmentData.WarehouseName = header.WarehouseName;
                                    shipmentData.DestinationName = header.DestinationName;
                                    shipmentData.TransporterName = header.TransporterName;
                                    shipmentData.DriverName = header.DriverName;
                                    shipmentData.PlateNumber = header.PlateNumber;
                                    shipmentData.TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(header.TransactionId, Constant.facelift_encryption_key));
                                    String body = RenderViewToString("Inbound", "WarningEmail", shipmentData);

                                    Mailing mailing = new Mailing();
                                    Task.Factory.StartNew(() => mailing.SendEmail(emails, "Facelift - Inbound Warning", body));
                                    header.WarningCount = 0;
                                }
                            }
                            else
                            {
                                //reset warning count
                                header.WarningCount = 0;
                            }

                            await IShipments.UpdateWarningCountAsync(header);
                            string actionName = string.Format("Receive {0} Item (RFID Gate)", totalScanned);

                            status = await IShipments.ReceiveItemAsync(header, username, actionName);
                            if (status)
                            {
                                if (totalRow == totalScanned)
                                {
                                    message = "Pallet receive successfuly.";
                                }
                                else
                                {
                                    message = totalScanned.ToString() + " Pallet receive parsial successfuly.";
                                }

                                string username_ext = "System";
                                IEnumerable<TrxShipmentHeader> list = await IShipments.GetDataAllInboundTransactionProgress();
                                if (list != null)
                                {
                                    foreach (var shipmentHeader in list)
                                    {
                                        TrxShipmentHeader headerext = await IShipments.GetDataByIdAsync(shipmentHeader.TransactionId);

                                        DateTime currentDate_ext = DateTime.Now;
                                        string[] items = headerext.TrxShipmentItems.Where(m => m.TransactionId.Equals(shipmentHeader.TransactionId)).Select(m => m.TagId).ToArray();

                                        List<TrxShipmentItem> detailext = headerext.TrxShipmentItems.ToList();
                                        foreach (var tag in items)
                                        {
                                            string tagId = Utilities.ConvertTag(tag);
                                            TrxShipmentItem item = detailext.Where(m => m.TagId.Equals(tagId)).FirstOrDefault();
                                            if (item != null)
                                            {
                                                if (item.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString()))
                                                {
                                                    item.ReceivedBy = username_ext;
                                                    item.ReceivedAt = currentDate_ext;
                                                    item.PalletMovementStatus = Constant.PalletMovementStatus.IN.ToString();
                                                    //update current data index
                                                    int index = detailext.IndexOf(item);
                                                    detailext[index] = item;
                                                }
                                            }
                                        }

                                        headerext.TransactionStatus = Constant.TransactionStatus.CLOSED.ToString();
                                        headerext.ShipmentStatus = Constant.ShipmentStatus.RECEIVE.ToString();

                                        // delete data temp by transaction id trxshipmentheader
                                        TrxShipmentItemTemp itemp = await IShipments.GetDataByTransactionIdTempAsync(header.TransactionId);
                                        if (itemp != null)
                                        {
                                            bool delete = await IShipments.DeleteItemTempAsync(itemp);
                                        }

                                        string actionName_ext = string.Format("Receive {0} Item (System)", detailext.Count());
                                        status = await IShipments.ReceiveItemAsync(headerext, username_ext, actionName_ext);
                                    }
                                }
                            }
                            else
                            {
                                message = "Pallet receive failed. Please contact system administrator.";
                            }
                        }
                        else
                        {
                            throw new Exception("Unrecognized warehouse for current transaction.");
                        }
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return Unauthorized();
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
            obj.Add("TransactionId", TransactionId);
            obj.Add("TotalPallet", totalUnscanned);
            obj.Add("QtyPalletShipmnet", totalQtyShipment);

            return Ok(obj);
        }

        public string RenderViewToString(string controllerName, string viewName, object viewData)
        {
            var context = HttpContext.Current;
            var contextBase = new HttpContextWrapper(context);
            var routeData = new RouteData();
            routeData.Values.Add("controller", controllerName);

            var controllerContext = new ControllerContext(contextBase,
                                                         routeData,
                                                         new EmptyController());

            var razorViewEngine = new RazorViewEngine();
            var razorViewResult = razorViewEngine.FindView(controllerContext,
                                                            viewName,
                                                            "",
                                                            false);

            var writer = new StringWriter();
            var viewContext = new ViewContext(controllerContext,
                                             razorViewResult.View,
                                             new ViewDataDictionary(viewData),
                                             new TempDataDictionary(),
                                             writer);
            razorViewResult.View.Render(viewContext, writer);

            return writer.ToString();
        }        
    }

    public class EmptyController : ControllerBase { protected override void ExecuteCore() { } }

}
