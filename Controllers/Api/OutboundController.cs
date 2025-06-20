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
    public class OutboundController : ApiController
    {
        private readonly IUsers IUsers;
        private readonly IMenus IMenus;
        private readonly IShipments IShipments;
        private readonly IWarehouses IWarehouses;
        private readonly IPallets IPallets;

        public OutboundController(IUsers Users, IMenus Menus, IShipments Shipments, IWarehouses Warehouses, IPallets Pallets)
        {
            IUsers = Users;
            IMenus = Menus;
            IShipments = Shipments;
            IWarehouses = Warehouses;
            IPallets = Pallets;
        }

        // GET api/values
        public async Task<IHttpActionResult> Get()
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "";
            bool status = false;
            var re = Request;
            var headers = re.Headers;

            try
            {
                //get user access
                if (headers.Contains(Constant.facelift_token_name) && headers.Contains("warehouseId"))
                {
                    string token = headers.GetValues(Constant.facelift_token_name).First();
                    string warehouseId = headers.GetValues("warehouseId").First();
                    string username = Encryptor.Decrypt(Utilities.DecodeFrom64(token), Constant.facelift_token_key);
                    MsUser user = await IUsers.GetDataByIdAsync(username);
                    MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(warehouseId);
                    if (user != null && warehouse != null)
                    {
                        IEnumerable<TrxShipmentHeader> list = IShipments.GetListOutbound(warehouseId);
                        IEnumerable<ShipmentHeaderDTO> shipments = from x in list
                                                                select new ShipmentHeaderDTO
                                                                {
                                                                    TransactionId = x.TransactionId,
                                                                    TransactionCode = x.TransactionCode,
                                                                    ShipmentNumber = !string.IsNullOrEmpty(x.ShipmentNumber) ? x.ShipmentNumber : "-",
                                                                    Remarks = !string.IsNullOrEmpty(x.Remarks) ? x.Remarks : "-",
                                                                    WarehouseName = x.WarehouseName,
                                                                    DestinationName = x.DestinationName,
                                                                    TransporterName = x.TransporterName,
                                                                    DriverName = x.DriverName,
                                                                    PlateNumber = x.PlateNumber,
                                                                    TransactionStatus = x.TransactionStatus,
                                                                    ShipmentStatus = x.ShipmentStatus,
                                                                    CreatedBy = x.CreatedBy,
                                                                    CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
                                                                    ModifiedBy = !string.IsNullOrEmpty(x.ModifiedBy) ? x.ModifiedBy : "-",
                                                                    ModifiedAt = Utilities.NullDateTimeToString(x.ModifiedAt),
                                                                    ApprovedBy = !string.IsNullOrEmpty(x.ApprovedBy) ? x.ApprovedBy : "-",
                                                                    ApprovedAt = Utilities.NullDateTimeToString(x.ApprovedAt)
                                                                };

                        obj.Add("shipments", shipments);
                        status = true;
                        message = "Retrieve shipment succeeded.";
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
            }

            obj.Add("status", status);
            obj.Add("message", message);

            return Ok(obj);
        }

        public async Task<IHttpActionResult> GetById(string transactionId)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "";
            bool status = false;
            var re = Request;
            var headers = re.Headers;

            try
            {
                //get user access
                if (headers.Contains(Constant.facelift_token_name) && headers.Contains("warehouseId"))
                {
                    string token = headers.GetValues(Constant.facelift_token_name).First();
                    string warehouseId = headers.GetValues("warehouseId").First();
                    string username = Encryptor.Decrypt(Utilities.DecodeFrom64(token), Constant.facelift_token_key);
                    MsUser user = await IUsers.GetDataByIdAsync(username);
                    MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(warehouseId);
                    if (user != null && warehouse != null)
                    {
                        TrxShipmentHeader header = await IShipments.GetDataByIdAsync(transactionId);
                        ShipmentHeaderDTO shipment = null;
                        if (header != null)
                        {
                            shipment = new ShipmentHeaderDTO
                            {
                                TransactionId = header.TransactionId,
                                TransactionCode = header.TransactionCode,
                                ShipmentNumber = !string.IsNullOrEmpty(header.ShipmentNumber) ? header.ShipmentNumber : "-",
                                Remarks = !string.IsNullOrEmpty(header.Remarks) ? header.Remarks : "-",
                                WarehouseName = header.WarehouseName,
                                DestinationName = header.DestinationName,
                                TransporterName = header.TransporterName,
                                DriverName = header.DriverName,
                                PlateNumber = header.PlateNumber,
                                TransactionStatus = header.TransactionStatus,
                                ShipmentStatus = header.ShipmentStatus,
                                CreatedBy = header.CreatedBy,
                                CreatedAt = Utilities.NullDateTimeToString(header.CreatedAt),
                                ModifiedBy = !string.IsNullOrEmpty(header.ModifiedBy) ? header.ModifiedBy : "-",
                                ModifiedAt = Utilities.NullDateTimeToString(header.ModifiedAt),
                                ApprovedBy = !string.IsNullOrEmpty(header.ApprovedBy) ? header.ApprovedBy : "-",
                                ApprovedAt = Utilities.NullDateTimeToString(header.ApprovedAt)
                            };

                            IEnumerable<VwShipmentItem> items = IShipments.GetDetailByTransactionId(transactionId).ToList();
                            shipment.items = from x in items
                                             select new ShipmentItemDTO
                                             {
                                                 TransactionItemId = x.TransactionItemId,
                                                 TagId = x.TagId,
                                                 PalletMovementStatus = x.PalletMovementStatus,
                                                 PalletTypeName = x.PalletName,
                                                 PalletOwner = x.PalletOwner,
                                                 PalletProducer = x.PalletProducer,
                                                 ScannedBy = x.ScannedBy,
                                                 ScannedAt = Utilities.NullDateTimeToString(x.ScannedAt)
                                             };
                        }

                        obj.Add("shipment", shipment);
                        status = true;
                        message = "Retrieve shipment succeeded.";
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
            }

            obj.Add("status", status);
            obj.Add("message", message);


            return Ok(obj);
        }

        public async Task<IHttpActionResult> Post(Shipment shipment)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "Invalid form submission.";
            bool status = false;
            var re = Request;
            var headers = re.Headers;
            try
            {
                if (ModelState.IsValid)
                {
                    //get user access
                    if (headers.Contains(Constant.facelift_token_name))
                    {
                        string token = headers.GetValues(Constant.facelift_token_name).First();
                        string username = Encryptor.Decrypt(Utilities.DecodeFrom64(token), Constant.facelift_token_key);
                        MsUser user = await IUsers.GetDataByIdAsync(username);
                        if (user != null)
                        {
                            TrxShipmentHeader header = await IShipments.GetDataByIdAsync(shipment.TransactionId);
                            if(header == null)
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

                            if (!header.ShipmentStatus.Equals(Constant.ShipmentStatus.LOADING.ToString()))
                            {
                                throw new Exception("Transaction already processed.");
                            }

                            if (!header.PalletQty.Equals(shipment.items.Count()))
                            {
                                throw new Exception(string.Format("Pallet Quantity must be equal to {0}.", header.PalletQty));
                            }

                            List<TrxShipmentItem> detail = header.TrxShipmentItems.ToList();

                            DateTime currentDate = DateTime.Now;
                            //remove duplicate
                            foreach (string tag in shipment.items)
                            {
                                string tagId = Utilities.ConvertTag(tag);

                                MsPallet cek_pallet = await IPallets.GetDataByTagIdAsync(tagId);
                                if (cek_pallet != null && !cek_pallet.WarehouseId.Equals(header.WarehouseId) && !cek_pallet.PalletCondition.Equals(Constant.PalletMovementStatus.OT.ToString()))
                                {
                                    status = await IShipments.UpdateItemAsync(header.WarehouseId, tagId);
                                }
                                else if (cek_pallet != null && cek_pallet.PalletCondition.Equals(Constant.PalletCondition.FREEZE.ToString()))
                                {
                                    status = await IShipments.UpdateItemAsync(header.WarehouseId, tagId);
                                }

                                MsPallet pallet = await IPallets.GetDataByTagWarehouseIdAsync(tagId, header.WarehouseId);
                                if (pallet != null && pallet.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())
                                    && pallet.PalletCondition.Equals(Constant.PalletCondition.GOOD.ToString()))
                                {
                                    TrxShipmentItem item = await IShipments.GetDataByTransactionTagIdAsync(header.TransactionId, tagId);
                                    if (item == null)
                                    {
                                        TrxShipmentItem item2 = header.TrxShipmentItems.Where(m => m.TagId.Equals(tagId)).FirstOrDefault();
                                        if(item2 == null)
                                        {
                                            item = new TrxShipmentItem();
                                            item.TransactionItemId = Utilities.CreateGuid("SHI");
                                            item.TransactionId = header.TransactionId;
                                            item.TagId = tagId;
                                            item.ScannedBy = username;
                                            item.ScannedAt = currentDate;
                                            item.PalletMovementStatus = Constant.PalletMovementStatus.OP.ToString();
                                            header.TrxShipmentItems.Add(item);
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
                                }

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

                            //if 1 or more item uploaded, transaction status will be changed as progress
                            if (header.TrxShipmentItems != null && header.TrxShipmentItems.Count() > 0)
                            {
                                header.TransactionStatus = Constant.TransactionStatus.PROGRESS.ToString();
                            }

                            status = await IShipments.InsertItemAsync(header, username, "Loading Item (Scan)");
                            if (status)
                            {
                                message = "Pallet loading successfuly.";
                            }
                            else
                            {
                                message = "Submit item failed. Please contact system administrator.";
                            }
                        }
                        else
                        {
                            return Unauthorized();
                        }
                    }
                    else
                    {
                        return Unauthorized();
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

        public async Task<IHttpActionResult> Put(Shipment shipment)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "Invalid form submission.";
            bool status = false;
            var re = Request;
            var headers = re.Headers;
            try
            {
                ModelState["shipment.items"].Errors.Clear();
                if (ModelState.IsValid)
                {
                    //get user access
                    if (headers.Contains(Constant.facelift_token_name))
                    {
                        string token = headers.GetValues(Constant.facelift_token_name).First();
                        string username = Encryptor.Decrypt(Utilities.DecodeFrom64(token), Constant.facelift_token_key);
                        MsUser user = await IUsers.GetDataByIdAsync(username);
                        if (user != null)
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

                            if (!header.ShipmentStatus.Equals(Constant.ShipmentStatus.LOADING.ToString()))
                            {
                                throw new Exception("Transaction already processed.");
                            }

                            //if 1 or more item uploaded, transaction status will be changed as progress
                            if (header.TrxShipmentItems != null && header.TrxShipmentItems.Count() > 0)
                            {
                                header.ShipmentStatus = Constant.ShipmentStatus.DISPATCH.ToString();
                            }
                            else
                            {
                                throw new Exception("Dispatch failed. No pallet found.");
                            }

                            if (header.PalletQty != header.TrxShipmentItems.Count())
                            {
                                throw new Exception(string.Format("Dispatch failed. Pallet Quantity must be equal to {0}.", header.PalletQty));
                            }

                            status = await IShipments.DispatchItemAsync(header, username, "Dispatch Item (Scan)");
                            if (status)
                            {
                                message = "Pallet dispatch successfuly.";
                            }
                            else
                            {
                                message = "Dispatch item failed. Please contact system administrator.";
                            }
                        }
                        else
                        {
                            return Unauthorized();
                        }
                    }
                    else
                    {
                        return Unauthorized();
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
