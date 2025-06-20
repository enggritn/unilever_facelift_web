using Facelift_App.Helper;
using Facelift_App.Models.Api;
using Facelift_App.Services;
using NLog;
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
    public class RecallController : ApiController
    {

        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

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
                    MsUser user = db.MsUsers.Where(m => m.Username.Equals(username)).FirstOrDefault();
                    MsWarehouse warehouse = db.MsWarehouses.Where(m => m.WarehouseId.Equals(warehouseId)).FirstOrDefault();
                    if (user != null && warehouse != null)
                    {
                        IQueryable<TrxRecallHeader> query = db.TrxRecallHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(warehouseId) && !x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()) && x.IsDeleted == false);
                        IEnumerable<TrxRecallHeader> list = null;
                        try
                        {
                            list = query.OrderByDescending(x => x.TransactionCode).ToList();
                        }
                        catch (Exception e)
                        {
                            string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                            logger.Error(e, errMsg);
                        }


                        IEnumerable<RecallHeaderDTO> list_header = from x in list
                                                                           select new RecallHeaderDTO
                                                                           {
                                                                               TransactionId = x.TransactionId,
                                                                               TransactionCode = x.TransactionCode,
                                                                               Remarks = !string.IsNullOrEmpty(x.Remarks) ? x.Remarks : "-",
                                                                               WarehouseCode = x.WarehouseCode,
                                                                               WarehouseName = x.WarehouseName,
                                                                               TransactionStatus = x.TransactionStatus,
                                                                               CreatedBy = x.CreatedBy,
                                                                               CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
                                                                               ModifiedBy = !string.IsNullOrEmpty(x.ModifiedBy) ? x.ModifiedBy : "-",
                                                                               ModifiedAt = Utilities.NullDateTimeToString(x.ModifiedAt),
                                                                               ApprovedBy = !string.IsNullOrEmpty(x.ApprovedBy) ? x.ApprovedBy : "-",
                                                                               ApprovedAt = Utilities.NullDateTimeToString(x.ApprovedAt)
                                                                           };

                        obj.Add("list_header", list_header);
                        status = true;
                        message = "Retrieve headers succeeded.";
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
                    MsUser user = db.MsUsers.Where(m => m.Username.Equals(username)).FirstOrDefault();
                    MsWarehouse warehouse = db.MsWarehouses.Where(m => m.WarehouseId.Equals(warehouseId)).FirstOrDefault();
                    if (user != null && warehouse != null)
                    {
                        TrxRecallHeader header = db.TrxRecallHeaders.Where(m => m.TransactionId.Equals(transactionId)).FirstOrDefault();
                        RecallHeaderDTO recall = null;

                        if (header != null)
                        {
                            recall = new RecallHeaderDTO
                            {
                                TransactionId = header.TransactionId,
                                TransactionCode = header.TransactionCode,
                                Remarks = !string.IsNullOrEmpty(header.Remarks) ? header.Remarks : "-",
                                WarehouseCode = header.WarehouseCode,
                                WarehouseName = header.WarehouseName,
                                TransactionStatus = header.TransactionStatus,
                                CreatedBy = header.CreatedBy,
                                CreatedAt = Utilities.NullDateTimeToString(header.CreatedAt),
                                ModifiedBy = !string.IsNullOrEmpty(header.ModifiedBy) ? header.ModifiedBy : "-",
                                ModifiedAt = Utilities.NullDateTimeToString(header.ModifiedAt),
                                ApprovedBy = !string.IsNullOrEmpty(header.ApprovedBy) ? header.ApprovedBy : "-",
                                ApprovedAt = Utilities.NullDateTimeToString(header.ApprovedAt)
                            };

                            IEnumerable<TrxRecallItem> items = db.TrxRecallItems.Where(m => m.TransactionId.Equals(transactionId)).ToList();
                            recall.items = from x in items
                                                 select new RecallItemDTO
                                                 {
                                                     TransactionItemId = x.TransactionItemId,
                                                     TransactionId = x.TransactionId,
                                                     TagId = x.TagId,
                                                     PreviousWarehouseId = x.PreviousWarehouseId,
                                                     PreviousWarehouseCode = x.PreviousWarehouseCode,
                                                     PreviousWarehouseName = x.PreviousWarehouseName,
                                                     ScannedBy = x.ScannedBy,
                                                     ScannedAt = Utilities.NullDateTimeToString(x.ScannedAt),
                                                     PreviousPalletCondition = x.PreviousPalletCondition,
                                                     NewPalletCondition = x.NewPalletCondition,
                                                     ModifiedBy = x.ModifiedBy,
                                                     ModifiedAt = Utilities.NullDateTimeToString(x.ModifiedAt)
                                                 };
                        }




                        obj.Add("recall", recall);
                        status = true;
                        message = "Retrieve recall succeeded.";
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

        public async Task<IHttpActionResult> Post(RecallVM dataVM)
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
                        MsUser user = db.MsUsers.Where(m => m.Username.Equals(username)).FirstOrDefault();
                        if (user != null)
                        {
                            TrxRecallHeader header = db.TrxRecallHeaders.Where(m => m.TransactionId.Equals(dataVM.TransactionId)).FirstOrDefault();
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
                            DateTime currentDate = DateTime.Now;
                            foreach (string tag in dataVM.items)
                            {
                                string tagId = Utilities.ConvertTag(tag);
                                MsPallet pallet = db.MsPallets.Where(m => m.TagId.Equals(tagId)).FirstOrDefault();
                                string[] conditions = { "ST", "OP" };
                                if (pallet != null && conditions.Contains(pallet.PalletMovementStatus)
                                    && pallet.PalletCondition.Equals(Constant.PalletCondition.DAMAGE.ToString()) || pallet.PalletCondition.Equals(Constant.PalletCondition.LOSS.ToString()))
                                {
                                    pallet.PalletMovementStatus = Constant.PalletMovementStatus.OT.ToString();
                                    pallet.LastTransactionCode = header.TransactionCode;
                                    pallet.LastTransactionDate = header.CreatedAt;
                                    pallet.LastTransactionName = Constant.TransactionName.RECALL.ToString();

                                    TrxRecallItem item = db.TrxRecallItems.Where(m => m.TransactionId.Equals(header.TransactionId) && m.TagId.Equals(tagId)).FirstOrDefault();
                                    if(item == null)
                                    {
                                        item = new TrxRecallItem();
                                        item.TransactionItemId = Utilities.CreateGuid("RCi");
                                        item.TransactionId = header.TransactionId;
                                        item.TagId = tagId;
                                        item.PreviousWarehouseId = pallet.WarehouseId;
                                        item.PreviousWarehouseCode = pallet.WarehouseCode;
                                        item.PreviousWarehouseName = pallet.WarehouseName;
                                        item.ScannedBy = username;
                                        item.ScannedAt = currentDate;
                                        item.PreviousPalletCondition = pallet.PalletCondition;

                                        string PalletNewCondition = "";

                                        Constant.PalletCondition condition = (Constant.PalletCondition)Enum.Parse(typeof(Constant.PalletCondition), pallet.PalletCondition, true);

                                        switch (condition)
                                        {
                                            case Constant.PalletCondition.DAMAGE:
                                                PalletNewCondition = Constant.PalletCondition.GOOD.ToString();
                                                break;
                                            case Constant.PalletCondition.LOSS:
                                                PalletNewCondition = Constant.PalletCondition.FOUND.ToString();
                                                break;
                                        }


                                        item.NewPalletCondition = PalletNewCondition;
                                        item.ModifiedBy = username;
                                        item.ModifiedAt = currentDate;

                                        header.TrxRecallItems.Add(item);
                                    }
                                }

                            }

                            //if 1 or more item uploaded, transaction status will be changed as progress
                            if (header.TrxRecallItems != null && header.TrxRecallItems.Count() > 0)
                            {
                                header.TransactionStatus = Constant.TransactionStatus.PROGRESS.ToString();
                            }

                            LogRecallHeader log = new LogRecallHeader
                            {
                                LogId = Utilities.CreateGuid("LOG"),
                                TransactionId = header.TransactionId,
                                ActionName = "Insert Item (Scan)",
                                ExecutedBy = username,
                                ExecutedAt = DateTime.Now
                            };


                            db.LogRecallHeaders.Add(log);



                            using (var transaction = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    await db.SaveChangesAsync();
                                    transaction.Commit();
                                    status = true;
                                }
                                catch (Exception e)
                                {
                                    transaction.Rollback();
                                    string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                                    logger.Error(e, errMsg);
                                }
                            }

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
