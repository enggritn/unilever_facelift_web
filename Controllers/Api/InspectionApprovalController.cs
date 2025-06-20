using Facelift_App.Helper;
using Facelift_App.Models.Api;
using Facelift_App.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Facelift_App.Controllers.Api
{
    public class InspectionApprovalController : ApiController
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public async Task<IHttpActionResult> Put(InspectionApprovalVM dataVM)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            string message = "Invalid form submission.";
            bool status = false;
            var re = Request;
            var headers = re.Headers;
            try
            {
                if (headers.Contains(Constant.facelift_token_name) && headers.Contains("warehouseId"))
                {
                    string token = headers.GetValues(Constant.facelift_token_name).First();
                    string warehouseId = headers.GetValues("warehouseId").First();
                    string username = Encryptor.Decrypt(Utilities.DecodeFrom64(token), Constant.facelift_token_key);
                    MsUser user = db.MsUsers.Where(m => m.Username.Equals(username)).FirstOrDefault();
                    MsWarehouse warehouse = db.MsWarehouses.Where(m => m.WarehouseId.Equals(warehouseId)).FirstOrDefault();
                    if (user != null && warehouse != null)
                    {
                        TrxInspectionHeader header = db.TrxInspectionHeaders.Where(m => m.TransactionId.Equals(dataVM.TransactionId)).FirstOrDefault();

                        if (header == null)
                        {
                            throw new Exception("Transaction not recognized.");
                        }

                        if (header.IsDeleted)
                        {
                            throw new Exception("Transaction already deleted.");
                        }

                        //if (header.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                        //{
                        //    throw new Exception("Transaction already closed.");
                        //}

                        if (!string.IsNullOrEmpty(header.ApprovalStatus))
                        {
                            throw new Exception("Transaction already processed.");
                        }

                        string PalletCondition = "";
                        switch (dataVM.Status)
                        {
                            case "APPROVED":
                                PalletCondition = header.Type;
                                header.ApprovalStatus = Constant.StatusApproval.APPROVED.ToString();
                                message = "Inspection approved.";
                                break;
                            case "REJECTED":
                                PalletCondition = Constant.PalletCondition.GOOD.ToString();
                                header.ApprovalStatus = Constant.StatusApproval.REJECTED.ToString();
                                message = "Inspection rejected.";
                                break;
                            default:
                                throw new Exception("Unrecognized status.");
                        };

                        header.ApprovedAt = DateTime.Now;
                        header.ApprovedBy = username;

                        foreach(TrxInspectionItem item in header.TrxInspectionItems)
                        {
                            MsPallet pallet = db.MsPallets.Where(m => m.TagId.Equals(item.TagId)).FirstOrDefault();
                            pallet.PalletCondition = PalletCondition;
                            pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                            pallet.WarehouseId = header.ApproverWarehouseId;
                            pallet.WarehouseCode = header.ApproverWarehouseCode;
                            pallet.WarehouseName = header.ApproverWarehouseName;
                        }

                        LogInspectionHeader log = new LogInspectionHeader
                        {
                            LogId = Utilities.CreateGuid("LOG"),
                            TransactionId = header.TransactionId,
                            ActionName = "Approval : " + header.ApprovalStatus,
                            ExecutedBy = username,
                            ExecutedAt = header.ApprovedAt.Value
                        };

                        using (var transaction = db.Database.BeginTransaction())
                        {
                            try
                            {
                                db.LogInspectionHeaders.Add(log);
                                db.SaveChanges();
                                transaction.Commit();
                                status = true;
                            }
                            catch (Exception e)
                            {
                                transaction.Rollback();
                                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                                logger.Error(e, errMsg);
                                throw new Exception("Submit data failed. Please contact system administrator.");
                            }
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
            catch (HttpRequestException reqpEx)
            {
                message = "Req Err : " + reqpEx.Message;
            }
            catch (HttpResponseException respEx)
            {
                message = "Resp Err : " + respEx.Message;
            }
            catch (Exception ex)
            {
                message = "Err : " + ex.Message;
            }


            obj.Add("status", status);
            obj.Add("message", message);



            return Ok(obj);
        }
    }
}
