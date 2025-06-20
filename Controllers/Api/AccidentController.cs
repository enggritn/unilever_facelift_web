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
    public class AccidentController : ApiController
    {
        private readonly IUsers IUsers;
        private readonly IMenus IMenus;
        private readonly IAccidents IAccidents;
        private readonly IWarehouses IWarehouses;
        private readonly IPallets IPallets;

        public AccidentController(IUsers Users, IMenus Menus, IAccidents Accidents, IWarehouses Warehouses, IPallets Pallets)
        {
            IUsers = Users;
            IMenus = Menus;
            IAccidents = Accidents;
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
                        IEnumerable<TrxAccidentHeader> list = IAccidents.GetList(warehouseId);
                        IEnumerable<AccidentHeaderDTO> accidents = from x in list
                                                                select new AccidentHeaderDTO
                                                                {
                                                                    TransactionId = x.TransactionId,
                                                                    TransactionCode = x.TransactionCode,
                                                                    AccidentType = x.AccidentType,
                                                                    Remarks = !string.IsNullOrEmpty(x.Remarks) ? x.Remarks : "-",
                                                                    WarehouseName = x.WarehouseName,
                                                                    TransactionStatus = x.TransactionStatus,
                                                                    CreatedBy = x.CreatedBy,
                                                                    CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
                                                                    ModifiedBy = !string.IsNullOrEmpty(x.ModifiedBy) ? x.ModifiedBy : "-",
                                                                    ModifiedAt = Utilities.NullDateTimeToString(x.ModifiedAt),
                                                                    ApprovedBy = !string.IsNullOrEmpty(x.ApprovedBy) ? x.ApprovedBy : "-",
                                                                    ApprovedAt = Utilities.NullDateTimeToString(x.ApprovedAt)
                                                                };

                        obj.Add("accidents", accidents);
                        status = true;
                        message = "Retrieve accident succeeded.";
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
                        TrxAccidentHeader header = await IAccidents.GetDataByIdAsync(transactionId);
                        AccidentHeaderDTO accident = null;
                        if (header != null)
                        {
                            accident = new AccidentHeaderDTO
                            {
                                TransactionId = header.TransactionId,
                                TransactionCode = header.TransactionCode,
                                AccidentType = header.AccidentType,
                                Remarks = !string.IsNullOrEmpty(header.Remarks) ? header.Remarks : "-",
                                WarehouseName = header.WarehouseName,
                                TransactionStatus = header.TransactionStatus,
                                CreatedBy = header.CreatedBy,
                                CreatedAt = Utilities.NullDateTimeToString(header.CreatedAt),
                                ModifiedBy = !string.IsNullOrEmpty(header.ModifiedBy) ? header.ModifiedBy : "-",
                                ModifiedAt = Utilities.NullDateTimeToString(header.ModifiedAt),
                                ApprovedBy = !string.IsNullOrEmpty(header.ApprovedBy) ? header.ApprovedBy : "-",
                                ApprovedAt = Utilities.NullDateTimeToString(header.ApprovedAt)
                            };

                            IEnumerable<VwAccidentItem> items = IAccidents.GetDetailByTransactionId(transactionId).ToList();
                            accident.items = from x in items
                                             select new AccidentItemDTO
                                             {
                                                 TransactionItemId = x.TransactionItemId,
                                                 TagId = x.TagId,
                                                 PalletTypeName = x.PalletName,
                                                 ReasonType = !string.IsNullOrEmpty(x.ReasonType) ? x.ReasonType : "-",
                                                 ReasonName = !string.IsNullOrEmpty(x.ReasonName) ? x.ReasonName : "-",
                                                 ScannedBy = !string.IsNullOrEmpty(x.ScannedBy) ? x.ScannedBy : "-",
                                                 ScannedAt = Utilities.NullDateTimeToString(x.ScannedAt)
                                             };
                        }

                        obj.Add("accident", accident);
                        status = true;
                        message = "Retrieve accident succeeded.";
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


            return Ok(obj);
        }

        public async Task<IHttpActionResult> Post(AccidentVM dataVM)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "Invalid form submission.";
            bool status = false;
            var re = Request;
            var headers = re.Headers;
            try
            {
                //if (Constant.ReasonList.ContainsKey(dataVM.ReasonName))
                //{
                //    string reasonType = Constant.ReasonList[dataVM.ReasonName].ToString();
                //    if (!reasonType.Equals(Constant.ReasonType.DAMAGE.ToString()))
                //    {
                //        throw new Exception("Reason not valid.");
                //    }
                //}
                //else
                //{
                //    throw new Exception("Reason not valid.");
                //}
                if (ModelState.IsValid)
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
                            TrxAccidentHeader data = new TrxAccidentHeader
                            {
                                TransactionId = Utilities.CreateGuid("BA"),
                                AccidentType = Constant.AccidentType.INSPECTION.ToString(),
                                Remarks = dataVM.Remarks,
                                WarehouseId = warehouse.WarehouseId,
                                WarehouseCode = warehouse.WarehouseCode,
                                WarehouseName = warehouse.WarehouseName,
                                TransactionStatus = Constant.TransactionStatus.OPEN.ToString(),
                                CreatedBy = username
                            };

                            status = await IAccidents.CreateAsync(data);
                            if (status)
                            {
                                message = "Create data succeeded.";
                            }
                            else
                            {
                                message = "Create data failed. Please contact system administrator.";
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

        public async Task<IHttpActionResult> Put(Accident accident)
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
                            TrxAccidentHeader header = await IAccidents.GetDataByIdAsync(accident.TransactionId);
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
                            DateTime currentDate = DateTime.Now;
                            foreach (string tag in accident.items)
                            {
                                string tagId = Utilities.ConvertTag(tag);
                                MsPallet pallet = await IPallets.GetDataByTagIdAsync(tagId);
                                if (pallet != null && pallet.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())
                                    && pallet.PalletCondition.Equals(Constant.PalletCondition.GOOD.ToString()))
                                {
                                    TrxAccidentItem item = await IAccidents.GetDataByTransactionTagIdAsync(header.TransactionId, tagId);
                                    if (item == null)
                                    {
                                        item = new TrxAccidentItem();
                                        item.TransactionItemId = Utilities.CreateGuid("BAI");
                                        item.TransactionId = header.TransactionId;
                                        item.TagId = tagId;
                                        item.ReasonName = "DAMAGE - SITE OPERATOR";
                                        item.ReasonType = Constant.ReasonType.DAMAGE.ToString();
                                        item.ScannedBy = username;
                                        item.ScannedAt = currentDate;

                                        header.TrxAccidentItems.Add(item);
                                    }
                                }
                               
                            }

                            //if 1 or more item uploaded, transaction status will be changed as progress
                            if (header.TrxAccidentItems != null && header.TrxAccidentItems.Count() > 0)
                            {
                                header.TransactionStatus = Constant.TransactionStatus.PROGRESS.ToString();
                            }

                            status = await IAccidents.InsertItemAsync(header, username, "Insert Item (Scan)");
                            if (status)
                            {
                                message = "Pallet data uploaded successfuly.";
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
    }
}
