using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using Facelift_App.Validators;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Controllers
{
    [SessionCheck]
    [AuthCheck]
    public class RegistrationController : Controller
    {
        private readonly IRegistrations IRegistrations;
        private readonly IPalletTypes IPalletTypes;
        private readonly IWarehouses IWarehouses;
        private readonly IPalletProducers IPalletProducers;
        private readonly IPallets IPallets;
        private readonly IUsers IUsers;
        private readonly ICompanies ICompanies;


        public RegistrationController(IRegistrations Registrations, IPalletTypes PalletTypes, IWarehouses Warehouses, IPalletProducers PalletProducers, IPallets Pallets, IUsers Users, ICompanies Companies)
        {
            IRegistrations = Registrations;
            IPalletTypes = PalletTypes;
            IWarehouses = Warehouses;
            IPalletProducers = PalletProducers;
            IPallets = Pallets;
            IUsers = Users;
            ICompanies = Companies;
            ViewBag.WarehouseDropdown = true;
        }

        public ActionResult Index()
        {
            ViewBag.TempMessage = TempData["TempMessage"];
            return View();
        }

        [HttpPost]
        public ActionResult Datatable()
        {
            string warehouseId = Session["warehouseAccess"].ToString();
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            string stats = Request["stats"];

            IEnumerable<TrxRegistrationHeader> list = IRegistrations.GetFilteredData(warehouseId, stats, search, sortDirection, sortName);
            IEnumerable<RegistrationDTO> pagedData = Enumerable.Empty<RegistrationDTO>();

            int recordsTotal = IRegistrations.GetTotalData(warehouseId, stats);
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new RegistrationDTO
                            {
                                TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionId, Constant.facelift_encryption_key)),
                                TransactionCode = x.TransactionCode,
                                DeliveryNote = !string.IsNullOrEmpty(x.DeliveryNote) ? x.DeliveryNote : "-",
                                //Description = !string.IsNullOrEmpty(x.Description) ? x.Description : "-",
                                PalletName = x.PalletName,
                                WarehouseName = x.WarehouseName,
                                PalletOwner = x.PalletOwner,
                                PalletProducer = x.PalletProducer,
                                ProducedDate = Utilities.NullDateToString(x.ProducedDate),
                                TransactionStatus = Utilities.TransactionStatusBadge(x.TransactionStatus),
                                CreatedBy = x.CreatedBy,
                                CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
                                ModifiedBy = !string.IsNullOrEmpty(x.ModifiedBy) ? x.ModifiedBy : "-",
                                ModifiedAt = Utilities.NullDateTimeToString(x.ModifiedAt),
                                ApprovedBy = !string.IsNullOrEmpty(x.ApprovedBy) ? x.ApprovedBy : "-",
                                ApprovedAt = Utilities.NullDateTimeToString(x.ApprovedAt)                             
                            };
            }


            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> Create()
        {
            ViewBag.TypeList = new SelectList(await IPalletTypes.GetAllAsync(), "PalletTypeId", "PalletName");

            RegistrationVM dataVM = new RegistrationVM();
            //pallet owner get from user login
            //MsUser user = await IUsers.GetDataByUsernameAsync(Session["username"].ToString());
            //dataVM.PalletOwner = Constant.facelift_pallet_owner;
            //dataVM.PalletOwner = user.MsCompany.CompanyAbb;

            //IEnumerable<MsProducer> producers = await IPalletProducers.GetAllAsync();
            //because only 1 pallet producer, just take first record.
            //MsProducer producer = producers.ToList()[0];
            //dataVM.PalletProducer = producer.ProducerName;

            dataVM.WarehouseId = Session["warehouseAccess"].ToString();
            MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(dataVM.WarehouseId);
            dataVM.WarehouseCode = warehouse.WarehouseCode;
            dataVM.WarehouseName = warehouse.WarehouseName;

            ViewBag.CompanyList = new SelectList(await ICompanies.GetAllAsync(), "CompanyId", "CompanyName");
            ViewBag.ProducerList = new SelectList(await IPalletProducers.GetAllAsync(), "ProducerName", "ProducerName");

            return View(dataVM);
        }

        private async Task SaveValidation(RegistrationVM dataVM)
        {
            if (!string.IsNullOrEmpty(dataVM.PalletTypeId))
            {
                PalletTypeValidator validator = new PalletTypeValidator(IPalletTypes);
                string PalletValid = await validator.IsTypeExist(dataVM.PalletTypeId);
                if (!PalletValid.Equals("true"))
                {
                    ModelState.AddModelError("PalletTypeId", PalletValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.CompanyId))
            {
                CompanyValidator validator = new CompanyValidator(ICompanies);
                string IsValid = await validator.IsExist(dataVM.CompanyId);
                if (!IsValid.Equals("true"))
                {
                    ModelState.AddModelError("CompanyId", IsValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.ProducerName))
            {
                ProducerValidator validator = new ProducerValidator(IPalletProducers);
                string IsValid = await validator.IsExist(dataVM.ProducerName);
                if (!IsValid.Equals("true"))
                {
                    ModelState.AddModelError("ProducerName", IsValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.WarehouseId))
            {
                ModelState["WarehouseId"].Errors.Clear();
                WarehouseValidator validator = new WarehouseValidator(IWarehouses);
                string WarehouseValid = await validator.IsWarehouseExist(dataVM.WarehouseId);
                if (!WarehouseValid.Equals("true"))
                {
                    ModelState.AddModelError("WarehouseId", WarehouseValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(RegistrationVM dataVM)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            bool result = false;
            string message = "Invalid form submission.";

            dataVM.WarehouseId = Session["warehouseAccess"].ToString();
            

            //server validation
            await SaveValidation(dataVM);


            if (ModelState.IsValid)
            {
                MsPalletType palletType = await IPalletTypes.GetDataByIdAsync(dataVM.PalletTypeId);
                MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(dataVM.WarehouseId);

            
                
                TrxRegistrationHeader data = new TrxRegistrationHeader
                {
                    TransactionId = Utilities.CreateGuid("REG"),
                    DeliveryNote = dataVM.DeliveryNote,
                    Description = dataVM.Description,
                    PalletTypeId = dataVM.PalletTypeId,
                    PalletName = palletType.PalletName,
                    WarehouseId = dataVM.WarehouseId,
                    WarehouseCode = warehouse.WarehouseCode,
                    WarehouseName = warehouse.WarehouseName,
                    //PalletOwner = Constant.facelift_pallet_owner,
                    ProducedDate = dataVM.ProducedDate,
                    TransactionStatus = Constant.TransactionStatus.OPEN.ToString(),
                    CreatedBy = Session["username"].ToString()
                };

                //pallet owner get from user login
                //MsUser user = await IUsers.GetDataByUsernameAsync(Session["username"].ToString());
                //data.PalletOwner = user.MsCompany.CompanyAbb;
                //data.CompanyId = user.MsCompany.CompanyId;

                //get company ID
                MsCompany msCompany = await ICompanies.GetDataByIdAsync(dataVM.CompanyId);
                MsProducer msProducer = await IPalletProducers.GetDataByIdAsync(dataVM.ProducerName);

                data.PalletOwner = msCompany.CompanyAbb;
                data.CompanyId = msCompany.CompanyId;
                data.PalletProducer = msProducer.ProducerName;


                result = await IRegistrations.CreateAsync(data);
                if (result)
                {
                    message = "Create data succeeded.";
                    TempData["TempMessage"] = message;
                    response.Add("transactionId", Utilities.EncodeTo64(Encryptor.Encrypt(data.TransactionId, Constant.facelift_encryption_key)));
                }
                else
                {
                    message = "Create data failed. Please contact system administrator.";
                }

            }


            response.Add("stat", result);
            response.Add("msg", message);

            return Json(response);
        }


        public async Task<ActionResult> Detail(string x)
        {
            string warehouseId = Session["warehouseAccess"].ToString();
            ViewBag.TempMessage = TempData["TempMessage"];
            string id = "";
            TrxRegistrationHeader data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = await IRegistrations.GetDataByIdAsync(id);
                    if (data == null)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        if (!data.WarehouseId.Equals(warehouseId) || data.IsDeleted)
                        {
                            throw new Exception();
                        }
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }

            RegistrationVM dataVM = new RegistrationVM
            {
                TransactionCode = data.TransactionCode,
                DeliveryNote = data.DeliveryNote,
                Description = data.Description,
                PalletTypeId = data.PalletTypeId,
                PalletTypeName = data.PalletName,
                WarehouseId = data.WarehouseId,
                WarehouseCode = data.WarehouseCode,
                WarehouseName = data.WarehouseName,
                PalletOwner = data.PalletOwner,
                PalletProducer = data.PalletProducer,
                ProducedDate = data.ProducedDate,
                TransactionStatus = Utilities.TransactionStatusBadge(data.TransactionStatus),
                CreatedBy = data.CreatedBy,
                CreatedAt = data.CreatedAt,
                ModifiedBy = data.ModifiedBy,
                ModifiedAt = data.ModifiedAt,
                ApprovedBy = data.ApprovedBy,
                ApprovedAt = data.ApprovedAt,
                logs = data.LogRegistrationHeaders.OrderBy(m => m.ExecutedAt).ToList(),
                versions = data.LogRegistrationDocuments.OrderBy(m => m.Version).ToList()
            };

            ViewBag.TransactionStatus = data.TransactionStatus;
            ViewBag.Id = x;
            ViewBag.TypeList = new SelectList(await IPalletTypes.GetAllAsync(), "PalletTypeId", "PalletName");
            return View(dataVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Detail(string x, RegistrationVM dataVM)
        {
            bool result = false;
            string message = "Invalid form submission.";
            string id = "";
            string warehouseId = Session["warehouseAccess"].ToString();
            try
            {
                if (string.IsNullOrEmpty(x))
                {
                    throw new Exception();
                }

                id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                dataVM.TransactionCode = id;
            }
            catch (Exception)
            {
                message = "Update data failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            TrxRegistrationHeader data = await IRegistrations.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }
            else
            {
                if (!data.WarehouseId.Equals(warehouseId) || data.IsDeleted)
                {
                    message = "Bad Request. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }

                if (data.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                {
                    message = "Update data not allowed, document already closed.";
                    return Json(new { stat = result, msg = message });
                }
            }

            dataVM.WarehouseId = data.WarehouseId;

            await SaveValidation(dataVM);

            if (ModelState.IsValid)
            {
                MsPalletType palletType = await IPalletTypes.GetDataByIdAsync(dataVM.PalletTypeId);

                data.PalletTypeId = dataVM.PalletTypeId;
                data.PalletName = palletType.PalletName;
                data.ProducedDate = dataVM.ProducedDate;
                data.DeliveryNote = dataVM.DeliveryNote;
                data.Description = dataVM.Description;
                data.ModifiedBy = Session["username"].ToString();

                result = await IRegistrations.UpdateAsync(data);

                if (result)
                {
                    message = "Update data succeeded.";
                    TempData["TempMessage"] = message;
                }
                else
                {
                    message = "Update data failed. Please contact system administrator.";
                }
            }

            return Json(new { stat = result, msg = message });
        }

        [HttpPost]
        public async Task<JsonResult> Close(string x)
        {
            bool result = false;
            string message = "";
            string id = "";
            string warehouseId = Session["warehouseAccess"].ToString();
            try
            {
                if (string.IsNullOrEmpty(x))
                {
                    throw new Exception();
                }

                id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
            }
            catch (Exception)
            {
                message = "Post document failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            TrxRegistrationHeader data = await IRegistrations.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }
            else
            {
                if (!data.WarehouseId.Equals(warehouseId) || data.IsDeleted)
                {
                    message = "Bad Request. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }
                //check previous data, close can be executed if status == on progress

                if (data.TrxRegistrationItems.Count() < 1 && !data.TransactionStatus.Equals(Constant.TransactionStatus.PROGRESS.ToString()))
                {
                    message = "Document not allowed to be posted.";
                    return Json(new { stat = result, msg = message });
                }
                else if(data.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                {
                    message = "Post document not allowed, document already closed.";
                    return Json(new { stat = result, msg = message });
                }
                
            }

            //posted only for new pallet only ? if there's one registered pallet exist, can not posted data ? clarify this.

            data.TransactionStatus = Constant.TransactionStatus.CLOSED.ToString();
            data.ApprovedBy = Session["username"].ToString();

            result = await IRegistrations.CloseAsync(data);
            //after close, insert pallet to master pallet
            if (result)
            {
                message = "Post document succeeded.";
                TempData["TempMessage"] = message;
            }
            else
            {
                message = "Post document failed. Please contact system administrator.";
            }



            return Json(new { stat = result, msg = message });
        }

        [HttpPost]
        public async Task<JsonResult> Delete(string x)
        {
            bool result = false;
            string message = "";
            string id = "";
            string warehouseId = Session["warehouseAccess"].ToString();
            try
            {
                if (string.IsNullOrEmpty(x))
                {
                    throw new Exception();
                }

                id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
            }
            catch (Exception)
            {
                message = "Delete document failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            TrxRegistrationHeader data = await IRegistrations.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }
            else
            {
                if (!data.WarehouseId.Equals(warehouseId) || data.IsDeleted)
                {
                    message = "Bad Request. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }

                if (data.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                {
                    message = "Delete document not allowed, document already closed.";
                    return Json(new { stat = result, msg = message });
                }
            }

            data.IsDeleted = true;

            data.ModifiedBy = Session["username"].ToString();

            result = await IRegistrations.DeleteAsync(data);

            if (result)
            {
                message = "Delete document succeeded.";
                TempData["TempMessage"] = message;
            }
            else
            {
                message = "Delete document failed. Please contact system administrator.";
            }

            return Json(new { stat = result, msg = message });
        }

        [HttpPost]
        public ActionResult DatatableItem(string id)
        {
            string transactionId = "";
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new Exception();
                }

                transactionId = Encryptor.Decrypt(Utilities.DecodeFrom64(id), Constant.facelift_encryption_key);

                string warehouseId = Session["warehouseAccess"].ToString();
                int draw = Convert.ToInt32(Request["draw"]);
                int start = Convert.ToInt32(Request["start"]);
                int length = Convert.ToInt32(Request["length"]);
                string search = Request["search[value]"];
                string orderCol = Request["order[0][column]"];
                string sortName = Request["columns[" + orderCol + "][name]"];
                string sortDirection = Request["order[0][dir]"];

                IEnumerable<VwRegistrationItem> list = IRegistrations.GetFilteredDataItem(transactionId, search, sortDirection, sortName);
                IEnumerable<RegistrationItemDTO> pagedData = Enumerable.Empty<RegistrationItemDTO>(); ;

                int recordsTotal = IRegistrations.GetTotalDataItem(transactionId);
                int recordsFilteredTotal = list.Count();


                list = list.Skip(start).Take(length).ToList();


                //re-format
                if (list != null && list.Count() > 0)
                {
                    pagedData = from x in list
                                select new RegistrationItemDTO
                                {
                                    TransactionItemId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionItemId, Constant.facelift_encryption_key)),
                                    TagId = x.TagId,
                                    Status = Utilities.RegistrationItemStatusBadge(x.ItemStatus),
                                    InsertedBy = x.InsertedBy,
                                    InsertedAt = Utilities.NullDateTimeToString(x.InsertedAt),
                                    InsertMethod = Utilities.InsertMethodBadge(x.InsertMethod)
                                };
                }


                return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                                JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { result = false, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
           
        }

        [HttpPost]
        public async Task<JsonResult> UploadItem(string x)
        {
            bool result = false;
            string message = "";
            string id = "";
            string warehouseId = Session["warehouseAccess"].ToString();
            try
            {
                if (string.IsNullOrEmpty(x))
                {
                    throw new Exception();
                }

                id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
            }
            catch (Exception)
            {
                message = "Upload item failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            TrxRegistrationHeader data = await IRegistrations.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }
            else
            {
                if (!data.WarehouseId.Equals(warehouseId) || data.IsDeleted)
                {
                    message = "Bad Request. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }

                if (data.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                {
                    message = "Upload data not allowed, document already closed.";
                    return Json(new { stat = result, msg = message });
                }
            }

            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase file = Request.Files[0];

                //if (file != null && file.ContentLength > 0 && (Path.GetExtension(file.FileName).ToLower() == ".xlsx" || Path.GetExtension(file.FileName).ToLower() == ".xls"))
                    if (file != null && file.ContentLength > 0 && Path.GetExtension(file.FileName).ToLower() == ".csv")
                {
                    if(file.ContentLength < (4 * 1024 * 1024))
                    {
                        try
                        {
                            //string fileName = Path.GetFileName(file.FileName);
                            //string filePath = Path.Combine(Server.MapPath("~/App_Data/TempUploads"), fileName);
                            //file.SaveAs(filePath);

                            //insertion logic here
                            StreamReader sr = new StreamReader(file.InputStream, System.Text.Encoding.Default);
                            string results = sr.ReadToEnd();
                            sr.Close();

                            string[] row = results.Split('\n');
                            
                            int totalUnique = 0;
                            List<string> tags = new List<string>();
                            //row
                            for (int i = 1; i < row.Length; i++)
                            {
                                //loop by col index
                                if (!string.IsNullOrEmpty(row[i]))
                                {
                                    string[] col = Regex.Split(row[i], ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                                    string val = col[0].Replace("\"", string.Empty);
                                    string tagId = Utilities.ConvertTag(val);
                                    if (!tags.Contains(tagId))
                                    {
                                        tags.Add(tagId);
                                    }
                                    else
                                    {
                                        totalUnique += 1;
                                    }
                                   
                                }
                            }

                            //if duplicate data exist in file, can not upload.
                            if(totalUnique == 0)
                            {
                                DateTime currentDate = DateTime.Now;
                                int totalData = 0;
                                foreach (string tag in tags)
                                {
                                    totalData += 1;
                                    TrxRegistrationItem item = await IRegistrations.GetDataByTransactionTagIdAsync(data.TransactionId, tag);
                                    if (item == null)
                                    {
                                        
                                        item = new TrxRegistrationItem();
                                        item.TransactionItemId = Utilities.CreateGuid("RGI");
                                        item.TransactionId = data.TransactionId;
                                        item.TagId = tag;
                                        item.InsertedBy = Session["username"].ToString();
                                        item.InsertedAt = currentDate;
                                        item.InsertMethod = Constant.InsertMethod.MANUAL.ToString();

                                        MsPallet pallet = await IPallets.GetDataByTagIdAsync(tag);
                                        if(pallet != null)
                                        {
                                            item.PalletTagId = pallet.TagId;
                                        }

                                        data.TrxRegistrationItems.Add(item);
                                    }
                                }

                                //if 1 or more item uploaded, transaction status will be changed as progress
                                if (data.TrxRegistrationItems != null && data.TrxRegistrationItems.Count() > 0)
                                {
                                    data.TransactionStatus = Constant.TransactionStatus.PROGRESS.ToString();
                                }

                                result = await IRegistrations.InsertItemAsync(data, Session["username"].ToString(), "Insert Item (Manual Upload)");
                                if (result)
                                {
                                    message = string.Format("{0} Pallet data uploaded successfuly.", totalData);
                                    TempData["TempMessage"] = message;
                                }
                                else
                                {
                                    message = "Upload item failed. Please contact system administrator.";
                                }
                            }
                            else
                            {
                                message = "Upload item failed. File contains duplicate value, please check and revise the file abd then try again.";
                            }

                           
                        }
                        catch (Exception)
                        {
                            message = "Upload item failed";
                        }
                    }
                    else
                    {
                        message = "Upload failed. Maximum allowed file size : 4MB ";
                    }
                    
                }
                else
                {
                    message = "Upload item failed. File is invalid.";
                }
            }
            else
            {
                message = "No file uploaded.";
            }
            return Json(new { stat = result, msg = message });
        }


        [HttpPost]
        public async Task<JsonResult> DeleteItem(string x, bool selectAll, string[] items)
        {
            bool result = false;
            string message = "";
            string id = "";
            string warehouseId = Session["warehouseAccess"].ToString();
            try
            {
                if (string.IsNullOrEmpty(x))
                {
                    throw new Exception();
                }

                id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
            }
            catch (Exception)
            {
                message = "Delete item failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            TrxRegistrationHeader data = await IRegistrations.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }
            else
            {
                if (!data.WarehouseId.Equals(warehouseId) || data.IsDeleted)
                {
                    message = "Bad Request. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }

                if (data.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                {
                    message = "Delete data not allowed, document already closed.";
                    return Json(new { stat = result, msg = message });
                }
            }

            if (selectAll)
            {
                items = data.TrxRegistrationItems.Select(m => m.TransactionItemId).ToArray();
                //if all data deleted, document status will become open
                data.TransactionStatus = Constant.TransactionStatus.OPEN.ToString();
            }
            else
            {
                try
                {
                    items = items.Select(s => Encryptor.Decrypt(Utilities.DecodeFrom64(s), Constant.facelift_encryption_key)).ToArray();
                }
                catch (Exception)
                {
                    message = "Delete item failed. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }
            }
            result = await IRegistrations.DeleteItemAsync(data, items, Session["username"].ToString());

            if (result)
            {
                message = "Delete item succeeded.";
                TempData["TempMessage"] = message;
            }
            else
            {
                message = "Delete item failed. Please contact system administrator.";
            }


            return Json(new { stat = result, msg = message });
        }

        public FileResult DownloadTemplate()
        {
            string filePath = "~/App_Data/template/registration_template.csv";
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath(filePath));
            string fileName = "(Facelift) Manual Registration Template.csv";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult ExportListToExcel()
        {
            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_Registration_{0}.xlsx", date);
            string warehouseId = Session["warehouseAccess"].ToString();
            IEnumerable<TrxRegistrationHeader> list = IRegistrations.GetAllTransactions(warehouseId);
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;
          

            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Transaction Code";
            workSheet.Cells[1, 2].Value = "Delivery Note";
            workSheet.Cells[1, 3].Value = "Description";
            workSheet.Cells[1, 4].Value = "Pallet Name";
            workSheet.Cells[1, 5].Value = "Warehouse Code";
            workSheet.Cells[1, 6].Value = "Warehouse Name";
            workSheet.Cells[1, 7].Value = "Pallet Owner";
            workSheet.Cells[1, 8].Value = "Pallet Producer";
            workSheet.Cells[1, 9].Value = "Produced Date";
            workSheet.Cells[1, 10].Value = "Transaction Status";
            workSheet.Cells[1, 11].Value = "Created By";
            workSheet.Cells[1, 12].Value = "Created At";
            workSheet.Cells[1, 13].Value = "Modified By";
            workSheet.Cells[1, 14].Value = "Modified At";
            workSheet.Cells[1, 15].Value = "Approved By";
            workSheet.Cells[1, 16].Value = "Approved At";

            int recordIndex = 2;
            foreach (TrxRegistrationHeader header in list)
            {
                workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
                workSheet.Cells[recordIndex, 2].Value = header.DeliveryNote;
                workSheet.Cells[recordIndex, 3].Value = header.Description;
                workSheet.Cells[recordIndex, 4].Value = header.PalletName;
                workSheet.Cells[recordIndex, 5].Value = header.WarehouseCode;
                workSheet.Cells[recordIndex, 6].Value = header.WarehouseName;
                workSheet.Cells[recordIndex, 7].Value = header.PalletOwner;
                workSheet.Cells[recordIndex, 8].Value = header.PalletProducer;
                workSheet.Cells[recordIndex, 9].Value = Utilities.NullDateToString(header.ProducedDate);
                workSheet.Cells[recordIndex, 10].Value = header.TransactionStatus;
                workSheet.Cells[recordIndex, 11].Value = header.CreatedBy;
                workSheet.Cells[recordIndex, 12].Value = Utilities.NullDateTimeToString(header.CreatedAt);
                workSheet.Cells[recordIndex, 13].Value = header.ModifiedBy;
                workSheet.Cells[recordIndex, 14].Value = Utilities.NullDateTimeToString(header.ModifiedAt);
                workSheet.Cells[recordIndex, 15].Value = header.ApprovedBy;
                workSheet.Cells[recordIndex, 16].Value = Utilities.NullDateTimeToString(header.ApprovedAt);
                recordIndex++;
            }

            for (int i = 1; i <= 16; i++)
            {
                workSheet.Column(i).AutoFit();
            }

            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;" + fileName);
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
            return RedirectToAction("Index");
        }

        public ActionResult ExportDetailListToExcel()
        {
            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_Registration_Details_{0}.xlsx", date);
            string warehouseId = Session["warehouseAccess"].ToString();
            IEnumerable<TrxRegistrationHeader> list = IRegistrations.GetAllTransactions(warehouseId);
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Transaction Code";
            workSheet.Cells[1, 2].Value = "Delivery Note";
            workSheet.Cells[1, 3].Value = "Description";
            workSheet.Cells[1, 4].Value = "Pallet Name";
            workSheet.Cells[1, 5].Value = "Warehouse Code";
            workSheet.Cells[1, 6].Value = "Warehouse Name";
            workSheet.Cells[1, 7].Value = "Pallet Owner";
            workSheet.Cells[1, 8].Value = "Pallet Producer";
            workSheet.Cells[1, 9].Value = "Produced Date";
            workSheet.Cells[1, 10].Value = "Transaction Status";
            workSheet.Cells[1, 11].Value = "Created By";
            workSheet.Cells[1, 12].Value = "Created At";
            workSheet.Cells[1, 13].Value = "Modified By";
            workSheet.Cells[1, 14].Value = "Modified At";
            workSheet.Cells[1, 15].Value = "Approved By";
            workSheet.Cells[1, 16].Value = "Approved At";
            workSheet.Cells[1, 17].Value = "Pallet Tag Id";
            workSheet.Cells[1, 18].Value = "Pallet Status";
            workSheet.Cells[1, 19].Value = "Inserted By";
            workSheet.Cells[1, 20].Value = "Inserted At";
            workSheet.Cells[1, 21].Value = "Insert Method";

            int recordIndex = 2;
            foreach (TrxRegistrationHeader header in list)
            {
                foreach (TrxRegistrationItem item in header.TrxRegistrationItems)
                {
                    workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
                    workSheet.Cells[recordIndex, 2].Value = header.DeliveryNote;
                    workSheet.Cells[recordIndex, 3].Value = header.Description;
                    workSheet.Cells[recordIndex, 4].Value = header.PalletName;
                    workSheet.Cells[recordIndex, 5].Value = header.WarehouseCode;
                    workSheet.Cells[recordIndex, 6].Value = header.WarehouseName;
                    workSheet.Cells[recordIndex, 7].Value = header.PalletOwner;
                    workSheet.Cells[recordIndex, 8].Value = header.PalletProducer;
                    workSheet.Cells[recordIndex, 9].Value = Utilities.NullDateToString(header.ProducedDate);
                    workSheet.Cells[recordIndex, 10].Value = header.TransactionStatus;
                    workSheet.Cells[recordIndex, 11].Value = header.CreatedBy;
                    workSheet.Cells[recordIndex, 12].Value = Utilities.NullDateTimeToString(header.CreatedAt);
                    workSheet.Cells[recordIndex, 13].Value = header.ModifiedBy;
                    workSheet.Cells[recordIndex, 14].Value = Utilities.NullDateTimeToString(header.ModifiedAt);
                    workSheet.Cells[recordIndex, 15].Value = header.ApprovedBy;
                    workSheet.Cells[recordIndex, 16].Value = Utilities.NullDateTimeToString(header.ApprovedAt);
                    workSheet.Cells[recordIndex, 17].Value = item.TagId;
                    workSheet.Cells[recordIndex, 18].Value = item.PalletTagId == null ? "NEW" : "REGISTERED/DUPLICATE";
                    workSheet.Cells[recordIndex, 19].Value = item.InsertedBy;
                    workSheet.Cells[recordIndex, 20].Value = Utilities.NullDateTimeToString(item.InsertedAt);
                    workSheet.Cells[recordIndex, 21].Value = item.InsertMethod;
                    
                    recordIndex++;
                }
            }

            for (int i = 1; i <= 21; i++)
            {
                workSheet.Column(i).AutoFit();
            }

            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;" + fileName);
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
            return RedirectToAction("Index");
        }

    }
}