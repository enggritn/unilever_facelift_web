using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using Facelift_App.Validators;
using OfficeOpenXml;
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
    public class InboundController : Controller
    {
        private readonly IShipments IShipments;
        private readonly IWarehouses IWarehouses;
        private readonly IPallets IPallets;
        private readonly IAccidents IAccidents;
        private readonly IUsers IUsers;


        public InboundController(IShipments Shipments, IWarehouses Warehouses, IPallets Pallets, IAccidents Accidents, IUsers Users)
        {
            IShipments = Shipments;
            IWarehouses = Warehouses;
            IPallets = Pallets;
            IAccidents = Accidents;
            IUsers = Users;
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

            IEnumerable<TrxShipmentHeader> list = IShipments.GetInboundData(warehouseId, stats, search, sortDirection, sortName);
            IEnumerable<ShipmentDTO> pagedData = Enumerable.Empty<ShipmentDTO>(); ;

            int recordsTotal = IShipments.GetTotalInboundData(warehouseId, stats);
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new ShipmentDTO
                            {
                                TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionId, Constant.facelift_encryption_key)),
                                TransactionCode = x.TransactionCode,
                                ShipmentNumber = !string.IsNullOrEmpty(x.ShipmentNumber) ? x.ShipmentNumber : "-",
                                WarehouseName = x.WarehouseName,
                                DestinationName = x.DestinationName,
                                TransporterName = x.TransporterName,
                                DriverName = x.DriverName,
                                PlateNumber = x.PlateNumber,
                                PalletQty = Utilities.FormatThousand(x.PalletQty),
                                TransactionStatus = Utilities.TransactionStatusBadge(x.TransactionStatus),
                                ShipmentStatus = Utilities.ShipmentStatusBadge(x.ShipmentStatus),
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


        public async Task<ActionResult> Detail(string x)
        {
            string warehouseId = Session["warehouseAccess"].ToString();
            ViewBag.TempMessage = TempData["TempMessage"];
            string id = "";
            TrxShipmentHeader data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = await IShipments.GetDataByIdAsync(id);
                    if (data == null)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        if (!data.DestinationId.Equals(warehouseId) || data.IsDeleted)
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

            ShipmentVM dataVM = new ShipmentVM
            {
                TransactionCode = data.TransactionCode,
                ShipmentNumber = data.ShipmentNumber,
                Remarks = data.Remarks,
                WarehouseId = data.WarehouseId,
                WarehouseCode = data.WarehouseCode,
                WarehouseName = data.WarehouseName,
                DestinationId = data.DestinationId,
                DestinationCode = data.DestinationCode,
                DestinationName = data.DestinationName,
                TransporterId = data.TransporterId,
                TransporterName = data.TransporterName,
                DriverId = data.DriverId,
                DriverName = data.DriverName,
                TruckId = data.TruckId,
                PlateNumber = data.PlateNumber,
                PalletQty = data.PalletQty,
                TransactionStatus = Utilities.TransactionStatusBadge(data.TransactionStatus),
                ShipmentStatus = Utilities.ShipmentStatusBadge(data.ShipmentStatus),
                CreatedBy = data.CreatedBy,
                CreatedAt = data.CreatedAt,
                ModifiedBy = data.ModifiedBy,
                ModifiedAt = data.ModifiedAt,
                ApprovedBy = data.ApprovedBy,
                ApprovedAt = data.ApprovedAt,
                logs = data.LogShipmentHeaders.OrderBy(m => m.ExecutedAt).ToList(),
                versions = data.LogShipmentDocuments.OrderBy(n => n.Version).ToList()
            };

            ViewBag.TransactionStatus = data.TransactionStatus;
            ViewBag.ShipmentStatus = data.ShipmentStatus;
            ViewBag.Id = x;

            MsUser user = await IUsers.GetDataByIdAsync(Session["username"].ToString());
            ViewBag.Role = user.RoleId;

            return View(dataVM);
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

                IEnumerable<VwShipmentItem> list = IShipments.GetFilteredDataItem(transactionId, search, sortDirection, sortName);
                IEnumerable<ShipmentItemDTO> pagedData = Enumerable.Empty<ShipmentItemDTO>();
                List<VwShipmentItem> listScanned = new List<VwShipmentItem>();

                int recordsTotal = IShipments.GetTotalDataItem(transactionId);
                int recordsFilteredTotal = list.Count();

                listScanned = list.Where(x => x.PalletMovementStatus.Equals(Constant.PalletMovementStatus.IN.ToString()) || x.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())).ToList();

                list = list.Skip(start).Take(length).ToList();


                //re-format
                if (list != null && list.Count() > 0)
                {
                    pagedData = from x in list
                                select new ShipmentItemDTO
                                {
                                    TransactionItemId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionItemId, Constant.facelift_encryption_key)),
                                    TagId = x.TagId,
                                    PalletMovementStatus = Utilities.MovementStatusBadge(x.PalletMovementStatus),
                                    PalletTypeName = x.PalletName,
                                    PalletOwner = x.PalletOwner,
                                    PalletProducer = x.PalletProducer,
                                    ReceivedBy = x.ReceivedBy,
                                    ReceivedAt = Utilities.NullDateTimeToString(x.ReceivedAt)
                                };
                }

                int totalScanned = listScanned.Count();

                return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData, totalScanned = totalScanned },
                                JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { result = false, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> CreateBA(string x)
        {
            string warehouseId = Session["warehouseAccess"].ToString();
            ViewBag.TempMessage = TempData["TempMessage"];
            string id = "";
            TrxShipmentHeader data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = await IShipments.GetDataByIdAsync(id);
                    if (data == null)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        if (!data.DestinationId.Equals(warehouseId) || data.IsDeleted)
                        {
                            throw new Exception();
                        }

                        if (data.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                        {
                            throw new Exception();
                        }
                        if (!string.IsNullOrEmpty(data.AccidentId))
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


            //AccidentVM dataVM = new AccidentVM
            //{
            //    TransactionCode = data.TransactionCode,
            //    ReasonType = Constant.ReasonList[reasonName].ToString(),
            //    ReasonName = reasonName,
            //    WarehouseId = data.DestinationId,
            //    WarehouseCode = data.DestinationCode,
            //    WarehouseName = data.DestinationName
            //};

            AccidentVM dataVM = new AccidentVM
            {
                TransactionCode = data.TransactionCode,
                AccidentType = Constant.AccidentType.INBOUND.ToString(),
                WarehouseId = data.DestinationId,
                WarehouseCode = data.DestinationCode,
                WarehouseName = data.DestinationName
            };

            ViewBag.Id = x;

            return View(dataVM);
        }

        private async Task SaveValidation(AccidentVM dataVM)
        {
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
        public async Task<JsonResult> ClosedShipment(string x)
        {
            bool status = false;
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
                message = "Closed shipment failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            TrxShipmentHeader header = await IShipments.GetDataByIdAsync(id);
            if (header == null)
            {
                message = "Transaction not recognized.";
                return Json(new { stat = result, msg = message });
            }

            if (header.IsDeleted)
            {
                message = "Transaction already deleted.";
                return Json(new { stat = result, msg = message });
            }

            if (header.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
            {
                message = "Transaction already closed.";
                return Json(new { stat = result, msg = message });
            }

            string username = "System";

            //check destination (inbound)
            if (header.DestinationId.Equals(warehouseId))
            {
                if (!header.ShipmentStatus.Equals(Constant.ShipmentStatus.DISPATCH.ToString()))
                {
                    message = "Delivery note is not valid.";
                    return Json(new { stat = result, msg = message });
                }

                DateTime currentDate = DateTime.Now;
                string[] items = header.TrxShipmentItems.Where(m => m.TransactionId.Equals(id)).Select(m => m.TagId).ToArray();

                List<TrxShipmentItem> detail = header.TrxShipmentItems.ToList();
                foreach (string tag in items)
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

                header.TransactionStatus = Constant.TransactionStatus.CLOSED.ToString();
                header.ShipmentStatus = Constant.ShipmentStatus.RECEIVE.ToString();

                // delete data temp by transaction id trxshipmentheader
                TrxShipmentItemTemp itemp = await IShipments.GetDataByTransactionIdTempAsync(header.TransactionId);
                if (itemp != null)
                {
                    bool delete = await IShipments.DeleteItemTempAsync(itemp);
                }

                string actionName = string.Format("Receive {0} Item (System)", detail.Count());
                status = await IShipments.ReceiveItemAsync(header, username, actionName);
                if (status)
                {
                    await AutoClosedShipment();
                    message = "Pallet receive successfuly.";
                }
                else
                {
                    message = "Pallet receive failed. Please contact system administrator.";
                }
            }

            return Json(new { stat = result, msg = message });
        }

        [HttpPost]
        public async Task AutoClosedShipment() //Task AutoClosedShipment()
        {
            bool status = false;           
            string username = "System";

            IEnumerable<TrxShipmentHeader> list = await IShipments.GetDataAllInboundTransactionProgress();
            if (list != null)
            {

                foreach (var shipmentHeader in list)
                {
                    TrxShipmentHeader header = await IShipments.GetDataByIdAsync(shipmentHeader.TransactionId);

                    DateTime currentDate = DateTime.Now;
                    string[] items = header.TrxShipmentItems.Where(m => m.TransactionId.Equals(shipmentHeader.TransactionId)).Select(m => m.TagId).ToArray();

                    List<TrxShipmentItem> detail = header.TrxShipmentItems.ToList();
                    foreach (var tag in items)
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

                    header.TransactionStatus = Constant.TransactionStatus.CLOSED.ToString();
                    header.ShipmentStatus = Constant.ShipmentStatus.RECEIVE.ToString();

                    // delete data temp by transaction id trxshipmentheader
                    TrxShipmentItemTemp itemp = await IShipments.GetDataByTransactionIdTempAsync(header.TransactionId);
                    if (itemp != null)
                    {
                        bool delete = await IShipments.DeleteItemTempAsync(itemp);
                    }

                    string actionName = string.Format("Receive {0} Item (System)", detail.Count());
                    status = await IShipments.ReceiveItemAsync(header, username, actionName);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateBA(string x, AccidentVM dataVM)
        {
            bool result = false;
            string message = "Invalid form submission.";
            string id = "";
            string warehouseId = Session["warehouseAccess"].ToString();
            string transactionId = "";
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
                message = "Create BA failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            TrxShipmentHeader shipment = await IShipments.GetDataByIdAsync(id);
            if (shipment == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }
            else
            {
                if (!shipment.DestinationId.Equals(warehouseId) || shipment.IsDeleted)
                {
                    message = "Bad Request. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }

                if (shipment.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                {
                    message = "Create BA data not allowed, document already closed.";
                    return Json(new { stat = result, msg = message });
                }

                if (!string.IsNullOrEmpty(shipment.AccidentId))
                {
                    message = "Create BA not allowed, BA already exist.";
                    return Json(new { stat = result, msg = message });
                }
            }
            string accidentType = Constant.AccidentType.INBOUND.ToString();

            //dataVM.ReasonType = Constant.ReasonList[reasonName].ToString();
            //dataVM.ReasonName = reasonName;
            dataVM.WarehouseId = warehouseId;

            ModelState["AccidentType"].Errors.Clear();

            await SaveValidation(dataVM);

            if (ModelState.IsValid)
            {
                MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(dataVM.WarehouseId);
                TrxAccidentHeader data = new TrxAccidentHeader
                {
                    TransactionId = Utilities.CreateGuid("BA"),
                    RefNumber = shipment.TransactionCode,
                    AccidentType = dataVM.AccidentType,
                    Remarks = dataVM.Remarks,
                    WarehouseId = dataVM.WarehouseId,
                    WarehouseCode = warehouse.WarehouseCode,
                    WarehouseName = warehouse.WarehouseName,
                    TransactionStatus = Constant.TransactionStatus.PROGRESS.ToString(),
                    CreatedBy = Session["username"].ToString()
                };

                string[] items = shipment.TrxShipmentItems.Where(m => m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString())).Select(m => m.TagId).ToArray();
                data.TrxAccidentItems = new List<TrxAccidentItem>();
                foreach (string tag in items)
                {
                    TrxAccidentItem item = new TrxAccidentItem();
                    item.TransactionItemId = Utilities.CreateGuid("BAI");
                    item.TransactionId = data.TransactionId;
                    item.TagId = tag;
                    //item.ScannedBy = Session["username"].ToString();
                    //item.ScannedAt = DateTime.Now;
                    data.TrxAccidentItems.Add(item);
                }

                //create BA
                result = await IAccidents.CreateAsync(data);
                if (result)
                {
                    //close transaction without updating stock
                    shipment.TransactionStatus = Constant.TransactionStatus.CLOSED.ToString();
                    shipment.ShipmentStatus = Constant.ShipmentStatus.RECEIVE.ToString();
                    shipment.AccidentId = data.TransactionCode;
                    result = await IShipments.CreateAccidentReportAsync(shipment, Session["username"].ToString());

                    if (result)
                    {
                        message = "Create BA succeeded.";
                        TempData["TempMessage"] = message;
                        transactionId = data.TransactionId;
                        TrxAccidentHeader accidentHeader = await IAccidents.GetDataByIdAsync(transactionId);

                        List<string> emailApprover = new List<string>();
                        MsUser pic1 = await IUsers.GetDataByUsernameAsync(shipment.MsWarehouse.PIC1);
                        MsUser pic2 = await IUsers.GetDataByUsernameAsync(shipment.MsWarehouse.PIC2);
                        if(pic1 != null)
                        {
                            emailApprover.Add(pic1.UserEmail);
                        }

                        if(pic2 != null)
                        {
                            emailApprover.Add(pic2.UserEmail);
                        }

                        List<string> emailTransporter = new List<string>();
                        emailTransporter.Add(shipment.MsTransporter.Email);

                        //send email here
                        ViewBag.ShipmentTransactionCode = data.RefNumber;
                        ViewBag.AccidentTransactionCode = accidentHeader.TransactionCode;
                        ViewBag.Quantity = Utilities.FormatThousand(data.TrxAccidentItems.Count);
                        ViewBag.ShipmentDate = Utilities.NullDateToString(shipment.CreatedAt);
                        ViewBag.Origin = shipment.MsWarehouse.WarehouseName;
                        ViewBag.Destination = shipment.MsWarehouse1.WarehouseName;
                        ViewBag.Transporter = shipment.TransporterName;
                        ViewBag.Driver = shipment.DriverName;
                        ViewBag.Truck = shipment.PlateNumber;
                        ViewBag.Id = Utilities.EncodeTo64(Encryptor.Encrypt(shipment.TransactionId, Constant.facelift_encryption_key));

                        String approver = Utilities.RenderViewToString(this.ControllerContext, "BAEmailApprover", null);
                        String transporter = Utilities.RenderViewToString(this.ControllerContext, "BAEmailTransporter", null);
                        Mailing mailing = new Mailing();
                        if(emailApprover.Count() > 0)
                        {
                            Task.Factory.StartNew(() => mailing.SendEmail(emailApprover, "Facelift - Informasi Berita Acara", approver));
                        }
                       
                        if(emailTransporter.Count() > 0)
                        {
                            Task.Factory.StartNew(() => mailing.SendEmail(emailTransporter, "Facelift - Informasi Berita Acara", transporter));
                        }

                    }
                    else
                    {
                        message = "Create BA failed. Please contact system administrator.";
                    }

                }
                else
                {
                    message = "Create BA failed. Please contact system administrator.";
                }
            }

            return Json(new { stat = result, msg = message, transactionId = transactionId });
        }


        public ActionResult BAEmail()
        {
            ViewBag.ShipmentTransactionCode = "0000";
            ViewBag.AccidentTransactionCode = "1111";
            ViewBag.Quantity = Utilities.FormatThousand(10);
            ViewBag.ShipmentDate = Utilities.NullDateToString(DateTime.Now);
            ViewBag.Origin = "CRMS";
            ViewBag.Destination = "SKIN";
            ViewBag.Transporter = "PT. DHL";
            ViewBag.Driver = "Bhov";
            ViewBag.Truck = "B 1010 KCC";
            return View("BAEmailApprover");
        }

        public ActionResult BAEmailTransporter()
        {
            ViewBag.ShipmentTransactionCode = "0000";
            ViewBag.AccidentTransactionCode = "1111";
            ViewBag.Quantity = Utilities.FormatThousand(10);
            ViewBag.ShipmentDate = Utilities.NullDateToString(DateTime.Now);
            ViewBag.Origin = "CRMS";
            ViewBag.Destination = "SKIN";
            ViewBag.Transporter = "PT. DHL";
            ViewBag.Driver = "Bhov";
            ViewBag.Truck = "B 1010 KCC";
            return View("BAEmailTransporter");
        }

        public ActionResult ExportListToExcel()
        {
            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_Inbound_{0}.xlsx", date);
            string warehouseId = Session["warehouseAccess"].ToString();
            IEnumerable<TrxShipmentHeader> list = IShipments.GetAllInboundTransactions(warehouseId);
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Transaction Code";
            workSheet.Cells[1, 2].Value = "Shipment Number";
            workSheet.Cells[1, 3].Value = "Remarks";
            workSheet.Cells[1, 4].Value = "Warehouse Code";
            workSheet.Cells[1, 5].Value = "Warehouse Name";
            workSheet.Cells[1, 6].Value = "Origin Code";
            workSheet.Cells[1, 7].Value = "Origin Name";
            workSheet.Cells[1, 8].Value = "Transporter Name";
            workSheet.Cells[1, 9].Value = "Driver Name";
            workSheet.Cells[1, 10].Value = "Plate Number";
            workSheet.Cells[1, 11].Value = "Pallet Qty";
            workSheet.Cells[1, 12].Value = "Transaction Status";
            workSheet.Cells[1, 13].Value = "Shipment Status";
            workSheet.Cells[1, 15].Value = "Created By";
            workSheet.Cells[1, 16].Value = "Created At";
            workSheet.Cells[1, 17].Value = "Modified By";
            workSheet.Cells[1, 18].Value = "Modified At";
            workSheet.Cells[1, 19].Value = "Approved By";
            workSheet.Cells[1, 20].Value = "Approved At";

            int recordIndex = 2;
            foreach (TrxShipmentHeader header in list)
            {
                workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
                workSheet.Cells[recordIndex, 2].Value = header.ShipmentNumber;
                workSheet.Cells[recordIndex, 3].Value = header.Remarks;
                workSheet.Cells[recordIndex, 4].Value = header.DestinationCode;
                workSheet.Cells[recordIndex, 5].Value = header.DestinationName;
                workSheet.Cells[recordIndex, 6].Value = header.WarehouseCode;
                workSheet.Cells[recordIndex, 7].Value = header.WarehouseName;
                workSheet.Cells[recordIndex, 8].Value = header.TransporterName;
                workSheet.Cells[recordIndex, 9].Value = header.DriverName;
                workSheet.Cells[recordIndex, 10].Value = header.PlateNumber;
                workSheet.Cells[recordIndex, 11].Value = header.PalletQty;
                workSheet.Cells[recordIndex, 12].Value = header.TransactionStatus;
                workSheet.Cells[recordIndex, 13].Value = header.ShipmentStatus;
                workSheet.Cells[recordIndex, 14].Value = header.CreatedBy;
                workSheet.Cells[recordIndex, 15].Value = Utilities.NullDateTimeToString(header.CreatedAt);
                workSheet.Cells[recordIndex, 16].Value = header.ModifiedBy;
                workSheet.Cells[recordIndex, 17].Value = Utilities.NullDateTimeToString(header.ModifiedAt);
                workSheet.Cells[recordIndex, 18].Value = header.ApprovedBy;
                workSheet.Cells[recordIndex, 19].Value = Utilities.NullDateTimeToString(header.ApprovedAt);
                recordIndex++;
            }

            for (int i = 1; i <= 19; i++)
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
            String fileName = String.Format("filename=Facelift_Inbound_Details_{0}.xlsx", date);
            string warehouseId = Session["warehouseAccess"].ToString();
            IEnumerable<TrxShipmentHeader> list = IShipments.GetAllInboundTransactions(warehouseId);
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Transaction Code";
            workSheet.Cells[1, 2].Value = "Shipment Number";
            workSheet.Cells[1, 3].Value = "Remarks";
            workSheet.Cells[1, 4].Value = "Warehouse Code";
            workSheet.Cells[1, 5].Value = "Warehouse Name";
            workSheet.Cells[1, 6].Value = "Origin Code";
            workSheet.Cells[1, 7].Value = "Origin Name";
            workSheet.Cells[1, 8].Value = "Transporter Name";
            workSheet.Cells[1, 9].Value = "Driver Name";
            workSheet.Cells[1, 10].Value = "Plate Number";
            workSheet.Cells[1, 11].Value = "Pallet Qty";
            workSheet.Cells[1, 12].Value = "Transaction Status";
            workSheet.Cells[1, 13].Value = "Shipment Status";
            workSheet.Cells[1, 14].Value = "Created By";
            workSheet.Cells[1, 15].Value = "Created At";
            workSheet.Cells[1, 16].Value = "Modified By";
            workSheet.Cells[1, 17].Value = "Modified At";
            workSheet.Cells[1, 18].Value = "Approved By";
            workSheet.Cells[1, 19].Value = "Approved At";
            workSheet.Cells[1, 20].Value = "Tag Id";
            workSheet.Cells[1, 21].Value = "Scanned By";
            workSheet.Cells[1, 22].Value = "Scanned At";
            workSheet.Cells[1, 23].Value = "Dispatched By";
            workSheet.Cells[1, 24].Value = "Dispatched At";
            workSheet.Cells[1, 25].Value = "Received By";
            workSheet.Cells[1, 26].Value = "Received At";
           

            int recordIndex = 2;
            foreach (TrxShipmentHeader header in list)
            {
                foreach (TrxShipmentItem item in header.TrxShipmentItems)
                {
                    workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
                    workSheet.Cells[recordIndex, 2].Value = header.ShipmentNumber;
                    workSheet.Cells[recordIndex, 3].Value = header.Remarks;
                    workSheet.Cells[recordIndex, 4].Value = header.DestinationCode;
                    workSheet.Cells[recordIndex, 5].Value = header.DestinationName;
                    workSheet.Cells[recordIndex, 6].Value = header.WarehouseCode;
                    workSheet.Cells[recordIndex, 7].Value = header.WarehouseName;
                    workSheet.Cells[recordIndex, 8].Value = header.TransporterName;
                    workSheet.Cells[recordIndex, 9].Value = header.DriverName;
                    workSheet.Cells[recordIndex, 10].Value = header.PlateNumber;
                    workSheet.Cells[recordIndex, 11].Value = header.PalletQty;
                    workSheet.Cells[recordIndex, 12].Value = header.TransactionStatus;
                    workSheet.Cells[recordIndex, 13].Value = header.ShipmentStatus;
                    workSheet.Cells[recordIndex, 14].Value = header.CreatedBy;
                    workSheet.Cells[recordIndex, 15].Value = Utilities.NullDateTimeToString(header.CreatedAt);
                    workSheet.Cells[recordIndex, 16].Value = header.ModifiedBy;
                    workSheet.Cells[recordIndex, 17].Value = header.ModifiedAt;
                    workSheet.Cells[recordIndex, 18].Value = header.ApprovedBy;
                    workSheet.Cells[recordIndex, 19].Value = Utilities.NullDateTimeToString(header.ApprovedAt);
                    workSheet.Cells[recordIndex, 20].Value = item.TagId;
                    workSheet.Cells[recordIndex, 21].Value = item.ScannedBy;
                    workSheet.Cells[recordIndex, 22].Value = Utilities.NullDateTimeToString(item.ScannedAt);
                    workSheet.Cells[recordIndex, 23].Value = item.DispatchedBy;
                    workSheet.Cells[recordIndex, 24].Value = Utilities.NullDateTimeToString(item.DispatchedAt);
                    workSheet.Cells[recordIndex, 25].Value = item.ReceivedBy;
                    workSheet.Cells[recordIndex, 26].Value = Utilities.NullDateTimeToString(item.ReceivedAt);
                    recordIndex++;
                }
            }

            for (int i = 1; i <= 26; i++)
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

        public ActionResult WarningEmail()
        {
            ShipmentDTO shipment = new ShipmentDTO();
            shipment.TransactionCode = "0000";
            shipment.PalletQty = Utilities.FormatThousand(10);
            shipment.CreatedAt = Utilities.NullDateToString(DateTime.Now);
            shipment.WarehouseName = "CRMS";
            shipment.DestinationName = "SKIN";
            shipment.TransporterName = "PT. DHL";
            shipment.DriverName = "Bhov";
            shipment.PlateNumber = "B 1010 KCC";
            return View(shipment);
        }
    }
}