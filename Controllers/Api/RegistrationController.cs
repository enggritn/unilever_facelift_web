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
    public class RegistrationController : ApiController
    {
        private readonly IUsers IUsers;
        private readonly IMenus IMenus;
        private readonly IRegistrations IRegistrations;
        private readonly IWarehouses IWarehouses;
        private readonly IPallets IPallets;

        public RegistrationController(IUsers Users, IMenus Menus, IRegistrations Registrations, IWarehouses Warehouses, IPallets Pallets)
        {
            IUsers = Users;
            IMenus = Menus;
            IRegistrations = Registrations;
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
                        IEnumerable<TrxRegistrationHeader> list = IRegistrations.GetAvailableTransactions(warehouseId);
                        IEnumerable<RegistrationHeaderDTO> registrations = from x in list
                                                                select new RegistrationHeaderDTO
                                                                {
                                                                    TransactionId = x.TransactionId,
                                                                    TransactionCode = x.TransactionCode,
                                                                    DeliveryNote = !string.IsNullOrEmpty(x.DeliveryNote) ? x.DeliveryNote : "-",
                                                                    Description = !string.IsNullOrEmpty(x.Description) ? x.Description : "-",
                                                                    PalletName = x.PalletName,
                                                                    WarehouseName = x.WarehouseName,
                                                                    PalletOwner = x.PalletOwner,
                                                                    PalletProducer = x.PalletProducer,
                                                                    ProducedDate = Utilities.NullDateToString(x.ProducedDate),
                                                                    TransactionStatus = x.TransactionStatus,
                                                                    CreatedBy = x.CreatedBy,
                                                                    CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
                                                                    ModifiedBy = !string.IsNullOrEmpty(x.ModifiedBy) ? x.ModifiedBy : "-",
                                                                    ModifiedAt = Utilities.NullDateTimeToString(x.ModifiedAt),
                                                                    ApprovedBy = !string.IsNullOrEmpty(x.ApprovedBy) ? x.ApprovedBy : "-",
                                                                    ApprovedAt = Utilities.NullDateTimeToString(x.ApprovedAt)
                                                                };

                        obj.Add("registrations", registrations);
                        status = true;
                        message = "Retrieve registration succeeded.";
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
                        TrxRegistrationHeader header = await IRegistrations.GetDataByIdAsync(transactionId);
                        RegistrationHeaderDTO registration = null;

                        if(header != null)
                        {
                            registration = new RegistrationHeaderDTO
                            {
                                TransactionId = header.TransactionId,
                                TransactionCode = header.TransactionCode,
                                DeliveryNote = !string.IsNullOrEmpty(header.DeliveryNote) ? header.DeliveryNote : "-",
                                Description = !string.IsNullOrEmpty(header.Description) ? header.Description : "-",
                                PalletName = header.PalletName,
                                WarehouseName = header.WarehouseName,
                                PalletOwner = header.PalletOwner,
                                PalletProducer = header.PalletProducer,
                                ProducedDate = Utilities.NullDateToString(header.ProducedDate),
                                TransactionStatus = header.TransactionStatus,
                                CreatedBy = header.CreatedBy,
                                CreatedAt = Utilities.NullDateTimeToString(header.CreatedAt),
                                ModifiedBy = !string.IsNullOrEmpty(header.ModifiedBy) ? header.ModifiedBy : "-",
                                ModifiedAt = Utilities.NullDateTimeToString(header.ModifiedAt),
                                ApprovedBy = !string.IsNullOrEmpty(header.ApprovedBy) ? header.ApprovedBy : "-",
                                ApprovedAt = Utilities.NullDateTimeToString(header.ApprovedAt)
                            };

                            IEnumerable<VwRegistrationItem> items = IRegistrations.GetDetailByTransactionId(transactionId).ToList();
                            registration.items = from x in items
                                                 select new RegistrationItemDTO
                                                 {
                                                     TransactionItemId = x.TransactionItemId,
                                                     TagId = x.TagId,
                                                     Status = x.ItemStatus,
                                                     InsertedBy = x.InsertedBy,
                                                     InsertedAt = Utilities.NullDateTimeToString(x.InsertedAt),
                                                     InsertMethod = x.InsertMethod
                                                 };
                        }
                        

                      

                        obj.Add("registration", registration);
                        status = true;
                        message = "Retrieve registration succeeded.";
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

        public async Task<IHttpActionResult> Post(Registration registration)
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
                            TrxRegistrationHeader registrationHeader = await IRegistrations.GetDataByIdAsync(registration.TransactionId);
                            if(registrationHeader == null)
                            {
                                throw new Exception("Transaction not recognized.");
                            }

                            if (registrationHeader.IsDeleted)
                            {
                                throw new Exception("Transaction already deleted.");
                            }

                            if (registrationHeader.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                            {
                                throw new Exception("Transaction already closed.");
                            }

                            DateTime currentDate = DateTime.Now;
                            foreach (string tag in registration.items)
                            {
                                string tagId = Utilities.ConvertTag(tag);
                                TrxRegistrationItem item = await IRegistrations.GetDataByTransactionTagIdAsync(registrationHeader.TransactionId, tagId);
                                if (item == null)
                                {
                                    item = new TrxRegistrationItem();
                                    item.TransactionItemId = Utilities.CreateGuid("RGI");
                                    item.TransactionId = registrationHeader.TransactionId;
                                    item.TagId = tagId;
                                    item.InsertedBy = username;
                                    item.InsertedAt = currentDate;
                                    item.InsertMethod = Constant.InsertMethod.SCAN.ToString();

                                    MsPallet pallet = await IPallets.GetDataByTagIdAsync(tagId);
                                    if (pallet != null)
                                    {
                                        item.PalletTagId = pallet.TagId;
                                    }

                                    registrationHeader.TrxRegistrationItems.Add(item);
                                }
                            }

                            //if 1 or more item uploaded, transaction status will be changed as progress
                            if (registrationHeader.TrxRegistrationItems != null && registrationHeader.TrxRegistrationItems.Count() > 0)
                            {
                                registrationHeader.TransactionStatus = Constant.TransactionStatus.PROGRESS.ToString();
                            }

                            status = await IRegistrations.InsertItemAsync(registrationHeader, username, "Insert Item (Scan Upload)");
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
