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
    public class InspectionController : ApiController
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
                        IQueryable<TrxInspectionHeader> query = db.TrxInspectionHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(warehouseId) && !x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()) && x.IsDeleted == false);
                        IEnumerable<TrxInspectionHeader> list = null;
                        try
                        {
                            list = query.OrderByDescending(x => x.TransactionCode).ToList();
                        }
                        catch (Exception e)
                        {
                            string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                            logger.Error(e, errMsg);
                        }


                        IEnumerable<InspectionHeaderDTO> list_header = from x in list
                                                                       select new InspectionHeaderDTO
                                                                       {
                                                                           TransactionId = x.TransactionId,
                                                                           TransactionCode = x.TransactionCode,
                                                                           Remarks = !string.IsNullOrEmpty(x.Remarks) ? x.Remarks : "-",
                                                                           RefNumber = x.RefNumber,
                                                                           Type = x.Type,
                                                                           PIC = x.PIC,
                                                                           Classification = x.Classification,
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
                        TrxInspectionHeader header = db.TrxInspectionHeaders.Where(m => m.TransactionId.Equals(transactionId)).FirstOrDefault();
                        InspectionHeaderDTO inspection = null;

                        if (header != null)
                        {
                            inspection = new InspectionHeaderDTO
                            {
                                TransactionId = header.TransactionId,
                                TransactionCode = header.TransactionCode,
                                Remarks = !string.IsNullOrEmpty(header.Remarks) ? header.Remarks : "-",
                                RefNumber = header.RefNumber,
                                PIC = header.PIC,
                                Classification = header.Classification,
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

                            IEnumerable<TrxInspectionItem> items = db.TrxInspectionItems.Where(m => m.TransactionId.Equals(transactionId)).ToList();
                            inspection.items = from x in items
                                           select new InspectionItemDTO
                                           {
                                               TransactionItemId = x.TransactionItemId,
                                               TransactionId = x.TransactionId,
                                               TagId = x.TagId,
                                               ScannedBy = x.ScannedBy,
                                               ScannedAt = Utilities.NullDateTimeToString(x.ScannedAt)
                                           };
                        }




                        obj.Add("inspection", inspection);
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

        public async Task<IHttpActionResult> Post(InspectionHeaderVM dataVM)
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

                        if (string.IsNullOrEmpty(dataVM.PIC))
                        {
                            throw new Exception("Please choose By Whom.");
                        }

                        if (string.IsNullOrEmpty(dataVM.Classification))
                        {
                            throw new Exception("Please choose Damage Classification.");
                        }

                        if (string.IsNullOrEmpty(dataVM.Remarks))
                        {
                            throw new Exception("Please input remarks.");
                        }
                        //manual validation check pallet
                        int palletQty = 0;
                        foreach (string tagId in dataVM.items)
                        {
                            MsPallet pallet = db.MsPallets.Where(m => m.TagId.Equals(tagId)).FirstOrDefault();
                            if (pallet != null && pallet.WarehouseId.Equals(warehouse.WarehouseId) && pallet.PalletCondition.Equals("GOOD") && pallet.PalletMovementStatus.Equals("ST"))
                            {
                                palletQty++;
                            }
                        }

                        if (palletQty < 1)
                        {
                            throw new Exception("Submit data failed. Please scan at least 1 pallet.");
                        }

                       

                        string PIC = Constant.InspectionPIC[dataVM.PIC].ToString();
                        string Classification = Constant.DamageClassification[dataVM.Classification].ToString();

                        bool sender = false;
                        if (PIC.Equals("Sender") || PIC.Equals("Transporter"))
                        {
                            sender = true;
                            //create multiple header based on warehouse last shipment
                        }

                        if (!sender)
                        {
                            TrxInspectionHeader data = new TrxInspectionHeader
                            {
                                TransactionId = Utilities.CreateGuid("QC"),
                                Type = Constant.ReasonType.DAMAGE.ToString(),
                                PIC = PIC,
                                Classification = Classification,
                                Remarks = dataVM.Remarks,
                                WarehouseId = warehouse.WarehouseId,
                                WarehouseCode = warehouse.WarehouseCode,
                                WarehouseName = warehouse.WarehouseName,
                                ApproverWarehouseId = warehouse.WarehouseId,
                                ApproverWarehouseCode = warehouse.WarehouseCode,
                                ApproverWarehouseName = warehouse.WarehouseName,
                                TransactionStatus = Constant.TransactionStatus.CLOSED.ToString(),
                                CreatedBy = username,
                                CreatedAt = DateTime.Now
                            };


                            string prefix = data.TransactionId.Substring(0, 2);
                            //string palletOwner = company.CompanyAbb;
                            string warehouseCode = data.WarehouseCode;
                            string warehouseAlias = warehouse.WarehouseAlias;
                            int year = Convert.ToInt32(data.CreatedAt.Year.ToString().Substring(2));
                            int month = data.CreatedAt.Month;
                            string romanMonth = Utilities.ConvertMonthToRoman(month);

                            // get last number, and do increment.
                            string lastNumber = db.TrxInspectionHeaders.AsQueryable().OrderByDescending(x => x.TransactionCode)
                                .Where(x => x.WarehouseCode.Equals(warehouseCode) && x.CreatedAt.Year.Equals(data.CreatedAt.Year) && x.CreatedAt.Month.Equals(data.CreatedAt.Month))
                                .AsEnumerable().Select(x => x.TransactionCode).FirstOrDefault();
                            int currentNumber = 0;

                            if (!string.IsNullOrEmpty(lastNumber))
                            {
                                currentNumber = Int32.Parse(lastNumber.Substring(lastNumber.Length - 3));
                            }

                            string runningNumber = string.Format("{0:D3}", currentNumber + 1);

                            data.TransactionCode = string.Format("{0}-{1}/{2}/{3}/{4}", prefix, warehouseAlias, year, romanMonth, runningNumber);

                            foreach (string tagId in dataVM.items)
                            {
                                MsPallet pallet = db.MsPallets.Where(m => m.TagId.Equals(tagId)).FirstOrDefault();
                                if (pallet != null && pallet.WarehouseId.Equals(warehouse.WarehouseId) && pallet.PalletCondition.Equals("GOOD") && pallet.PalletMovementStatus.Equals("ST"))
                                {
                                    TrxInspectionItem detail = new TrxInspectionItem()
                                    {
                                        TransactionItemId = Utilities.CreateGuid("QCi"),
                                        TransactionId = data.TransactionId,
                                        TagId = tagId,
                                        ScannedBy = username,
                                        ScannedAt = data.CreatedAt,
                                    };

                                    data.TrxInspectionItems.Add(detail);


                                    //update mspallet
                                    pallet.PalletMovementStatus = Constant.PalletMovementStatus.OT.ToString();
                                    pallet.LastTransactionCode = data.TransactionCode;
                                    pallet.LastTransactionDate = data.CreatedAt;
                                    pallet.LastTransactionName = Constant.TransactionName.INSPECTION.ToString();

                                }


                            }

                           

                            LogInspectionHeader log = new LogInspectionHeader
                            {
                                LogId = Utilities.CreateGuid("LOG"),
                                TransactionId = data.TransactionId,
                                ActionName = "Create",
                                ExecutedBy = data.CreatedBy,
                                ExecutedAt = data.CreatedAt
                            };

                            LogInspectionDocument logDoc = new LogInspectionDocument
                            {
                                LogId = Utilities.CreateGuid("LOG"),
                                TransactionId = data.TransactionId,
                                TransactionCode = data.TransactionCode,
                                Type = data.Classification,
                                PIC = data.PIC,
                                Classification = Classification,
                                Remarks = dataVM.Remarks,
                                WarehouseId = warehouse.WarehouseId,
                                WarehouseCode = warehouse.WarehouseCode,
                                WarehouseName = warehouse.WarehouseName,
                                ApproverWarehouseId = data.ApproverWarehouseId,
                                ApproverWarehouseCode = data.ApproverWarehouseCode,
                                ApproverWarehouseName = data.ApproverWarehouseName,
                                TransactionStatus = Constant.TransactionStatus.OPEN.ToString(),
                                CreatedBy = username,
                                CreatedAt = data.CreatedAt,
                                Version = 1
                            };
                          
                            using (var transaction = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    db.TrxInspectionHeaders.Add(data);
                                    db.LogInspectionHeaders.Add(log);
                                    db.LogInspectionDocuments.Add(logDoc);
                                    db.SaveChanges();
                                    transaction.Commit();
                                    status = true;
                                    message = "Submit data succeeded.";
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
                            List<TrxInspectionHeader> inspectionHeaders = new List<TrxInspectionHeader>();

                            //if multiple warehouse because sender
                            foreach (string tagId in dataVM.items)
                            {
                                MsPallet pallet = db.MsPallets.Where(m => m.TagId.Equals(tagId)).FirstOrDefault();
                                if (pallet != null && pallet.WarehouseId.Equals(warehouse.WarehouseId) && pallet.PalletCondition.Equals("GOOD") && pallet.PalletMovementStatus.Equals("ST"))
                                {
                                    //check if sender check last origin
                                    //loop check all pallet on the same sender
                                    TrxShipmentItem lastShipment = db.TrxShipmentItems.Where(m => m.TagId.Equals(tagId)).OrderByDescending(m => m.ReceivedAt).FirstOrDefault();
                                    //get transactioncode for refrence
                                    string TransactionCode = lastShipment.TrxShipmentHeader.TransactionCode;

                                    TrxInspectionHeader data = inspectionHeaders.Where(m => TransactionCode.Equals(m.RefNumber)).FirstOrDefault();
                                    if(data == null)
                                    {
                                        data = new TrxInspectionHeader
                                        {
                                            TransactionId = Utilities.CreateGuid("QC"),
                                            Type = Constant.ReasonType.DAMAGE.ToString(),
                                            RefNumber = TransactionCode,
                                            PIC = PIC,
                                            Classification = Classification,
                                            Remarks = dataVM.Remarks,
                                            WarehouseId = warehouse.WarehouseId,
                                            WarehouseCode = warehouse.WarehouseCode,
                                            WarehouseName = warehouse.WarehouseName,
                                            ApproverWarehouseId = lastShipment.TrxShipmentHeader.WarehouseId,
                                            ApproverWarehouseCode = lastShipment.TrxShipmentHeader.WarehouseCode,
                                            ApproverWarehouseName = lastShipment.TrxShipmentHeader.WarehouseName,
                                            TransactionStatus = Constant.TransactionStatus.CLOSED.ToString(),
                                            CreatedBy = username,
                                            CreatedAt = DateTime.Now
                                        };


                                        TrxInspectionItem detail = new TrxInspectionItem()
                                        {
                                            TransactionItemId = Utilities.CreateGuid("QCi"),
                                            TransactionId = data.TransactionId,
                                            TagId = tagId,
                                            ScannedBy = username,
                                            ScannedAt = data.CreatedAt,
                                        };

                                        data.TrxInspectionItems.Add(detail);

                                        inspectionHeaders.Add(data);

                                    }
                                    else
                                    {
                                        TrxInspectionItem detail = new TrxInspectionItem()
                                        {
                                            TransactionItemId = Utilities.CreateGuid("QCi"),
                                            TransactionId = data.TransactionId,
                                            TagId = tagId,
                                            ScannedBy = username,
                                            ScannedAt = data.CreatedAt,
                                        };

                                        data.TrxInspectionItems.Add(detail);
                                    }
                                }
                            }
                            

                            //loop insert
                            foreach(TrxInspectionHeader data in inspectionHeaders)
                            {
                                string prefix = data.TransactionId.Substring(0, 2);
                                //string palletOwner = company.CompanyAbb;
                                MsWarehouse approverWarehouse = db.MsWarehouses.Where(m => m.WarehouseId.Equals(data.ApproverWarehouseId)).FirstOrDefault();
                                string warehouseCode = approverWarehouse.WarehouseCode;
                                string warehouseAlias = approverWarehouse.WarehouseAlias;
                                int year = Convert.ToInt32(data.CreatedAt.Year.ToString().Substring(2));
                                int month = data.CreatedAt.Month;
                                string romanMonth = Utilities.ConvertMonthToRoman(month);

                                // get last number, and do increment.
                                string lastNumber = db.TrxInspectionHeaders.AsQueryable().OrderByDescending(x => x.TransactionCode)
                                    .Where(x => x.WarehouseCode.Equals(warehouseCode) && x.CreatedAt.Year.Equals(data.CreatedAt.Year) && x.CreatedAt.Month.Equals(data.CreatedAt.Month))
                                    .AsEnumerable().Select(x => x.TransactionCode).FirstOrDefault();
                                int currentNumber = 0;

                                if (!string.IsNullOrEmpty(lastNumber))
                                {
                                    currentNumber = Int32.Parse(lastNumber.Substring(lastNumber.Length - 3));
                                }

                                string runningNumber = string.Format("{0:D3}", currentNumber + 1);

                                data.TransactionCode = string.Format("{0}-{1}/{2}/{3}/{4}", prefix, warehouseAlias, year, romanMonth, runningNumber);

                                foreach(TrxInspectionItem item in data.TrxInspectionItems)
                                {
                                    MsPallet pallet = db.MsPallets.Where(m => m.TagId.Equals(item.TagId)).FirstOrDefault();
                                    if (pallet != null && pallet.WarehouseId.Equals(warehouse.WarehouseId) && pallet.PalletCondition.Equals("GOOD") && pallet.PalletMovementStatus.Equals("ST"))
                                    {
                                        //update mspallet
                                        pallet.PalletMovementStatus = Constant.PalletMovementStatus.OT.ToString();
                                        pallet.LastTransactionCode = data.TransactionCode;
                                        pallet.LastTransactionDate = data.CreatedAt;
                                        pallet.LastTransactionName = Constant.TransactionName.INSPECTION.ToString();
                                    }
                                }
                              

                                LogInspectionHeader log = new LogInspectionHeader
                                {
                                    LogId = Utilities.CreateGuid("LOG"),
                                    TransactionId = data.TransactionId,
                                    ActionName = "Create",
                                    ExecutedBy = data.CreatedBy,
                                    ExecutedAt = data.CreatedAt
                                };

                                LogInspectionDocument logDoc = new LogInspectionDocument
                                {
                                    LogId = Utilities.CreateGuid("LOG"),
                                    TransactionId = data.TransactionId,
                                    TransactionCode = data.TransactionCode,
                                    Type = data.Classification,
                                    PIC = data.PIC,
                                    Classification = Classification,
                                    Remarks = dataVM.Remarks,
                                    WarehouseId = warehouse.WarehouseId,
                                    WarehouseCode = warehouse.WarehouseCode,
                                    WarehouseName = warehouse.WarehouseName,
                                    ApproverWarehouseId = data.ApproverWarehouseId,
                                    ApproverWarehouseCode = data.ApproverWarehouseCode,
                                    ApproverWarehouseName = data.ApproverWarehouseName,
                                    TransactionStatus = Constant.TransactionStatus.OPEN.ToString(),
                                    CreatedBy = username,
                                    CreatedAt = data.CreatedAt,
                                    Version = 1
                                };


                                using (var transaction = db.Database.BeginTransaction())
                                {
                                    try
                                    {
                                        db.TrxInspectionHeaders.Add(data);
                                        db.LogInspectionHeaders.Add(log);
                                        db.LogInspectionDocuments.Add(logDoc);
                                        db.SaveChanges();
                                        transaction.Commit();
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

                            status = true;
                            message = "Submit data succeeded.";
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
