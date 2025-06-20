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
    public class CycleCountController : ApiController
    {
        private readonly IUsers IUsers;
        private readonly IMenus IMenus;
        private readonly ICycleCounts ICycleCounts;
        private readonly IWarehouses IWarehouses;
        private readonly IPallets IPallets;

        public CycleCountController(IUsers Users, IMenus Menus, ICycleCounts CycleCounts, IWarehouses Warehouses, IPallets Pallets)
        {
            IUsers = Users;
            IMenus = Menus;
            ICycleCounts = CycleCounts;
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
                        IEnumerable<TrxCycleCountHeader> list = ICycleCounts.GetList(warehouseId);
                        IEnumerable<CycleCountHeaderDTO> cycles = from x in list
                                                                select new CycleCountHeaderDTO
                                                                {
                                                                    TransactionId = x.TransactionId,
                                                                    TransactionCode = x.TransactionCode,
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

                        obj.Add("cycles", cycles);
                        status = true;
                        message = "Retrieve cycle count succeeded.";
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
                        TrxCycleCountHeader header = await ICycleCounts.GetDataByIdAsync(transactionId);
                        CycleCountHeaderDTO cycle = null;
                        if (header != null)
                        {
                            cycle = new CycleCountHeaderDTO
                            {
                                TransactionId = header.TransactionId,
                                TransactionCode = header.TransactionCode,
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

                            IEnumerable<VwCycleCountItem> items = ICycleCounts.GetDetailByTransactionId(transactionId).ToList();
                            cycle.items = from x in items
                                          select new CycleCountItemDTO
                                          {
                                              TransactionItemId = x.TransactionItemId,
                                              TagId = x.TagId,
                                              PalletCondition = x.PalletCondition,
                                              PalletMovementStatus = x.PalletMovementStatus,
                                              PalletTypeName = x.PalletName,
                                              PalletOwner = x.PalletOwner,
                                              PalletProducer = x.PalletProducer,
                                              ScannedBy = !string.IsNullOrEmpty(x.ScannedBy) ? x.ScannedBy : "-",
                                              ScannedAt = Utilities.NullDateTimeToString(x.ScannedAt)
                                          };
                        }

                        obj.Add("cycle", cycle);
                        status = true;
                        message = "Retrieve cycle count succeeded.";
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

        public async Task<IHttpActionResult> Post(CycleCount cycle)
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
                            TrxCycleCountHeader header = await ICycleCounts.GetDataByIdAsync(cycle.TransactionId);
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
                            List<TrxCycleCountItem> detail = header.TrxCycleCountItems.ToList();
                            foreach (string tag in cycle.items)
                            {
                                string tagId = Utilities.ConvertTag(tag);
                                TrxCycleCountItem item = detail.Where(m => m.TagId.Equals(tagId)).FirstOrDefault();
                                if (item != null)
                                {
                                    if (item.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OP.ToString()))
                                    {
                                        item.PalletCondition = item.PalletCondition.Equals(Constant.PalletCondition.LOSS.ToString()) ? "FOUND" : Constant.PalletCondition.GOOD.ToString();
                                        item.ScannedBy = username;
                                        item.ScannedAt = currentDate;
                                        item.PalletMovementStatus = Constant.PalletMovementStatus.OT.ToString();
                                        //update current data index
                                        int index = detail.IndexOf(item);
                                        detail[index] = item;
                                    }
                                }
                                else
                                {
                                    MsPallet pallet = await IPallets.GetDataByTagIdAsync(tagId);
                                    //extra found
                                    if (pallet != null && !pallet.PalletCondition.Equals(Constant.PalletCondition.DAMAGE.ToString()))
                                    {
                                        item = new TrxCycleCountItem();
                                        item.TransactionItemId = Utilities.CreateGuid("STI");
                                        item.TransactionId = header.TransactionId;
                                        item.TagId = tagId;
                                        item.PalletCondition = pallet.PalletCondition.Equals(Constant.PalletCondition.LOSS.ToString()) ? Constant.PalletCondition.FOUND.ToString() : Constant.PalletCondition.GOOD.ToString();
                                        item.ScannedBy = username;
                                        item.ScannedAt = currentDate;
                                        item.PalletMovementStatus = Constant.PalletMovementStatus.IN.ToString();
                                        //update pallet status in pallet stock, stock validation
                                        header.TrxCycleCountItems.Add(item);

                                        // update Warehouse IN
                                        status = await ICycleCounts.UpdatePalletAsync(header.WarehouseId, tagId, pallet.PalletCondition.Equals(Constant.PalletCondition.LOSS.ToString()) ? Constant.PalletCondition.FOUND.ToString() : Constant.PalletCondition.GOOD.ToString());
                                    }
                                }
                            }

                            int totalRow = detail.Count();
                            int totalScanned = detail.Where(m => m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString())).Count();
                                                        
                            if (header.TrxCycleCountItems != null && header.TrxCycleCountItems.Count() > 0)
                            {
                                header.TransactionStatus = Constant.TransactionStatus.PROGRESS.ToString();
                            }

                            status = await ICycleCounts.UpdateItemAsync(header, cycle.items, username, "Check Item (Scan)");
                            if (status)
                            {
                                message = "Pallet cycle count successfuly.";
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
