using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using Facelift_App.Validators;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ZXing;
using ZXing.QrCode;

namespace Facelift_App.Controllers
{
    [SessionCheck]
    [AuthCheck]
    public class OutboundController : Controller
    {
        private readonly IShipments IShipments;
        private readonly IWarehouses IWarehouses;
        private readonly IPallets IPallets;
        private readonly ITransporters ITransporters;
        private readonly ITransporterDrivers IDrivers;
        private readonly ITransporterTrucks ITrucks;
        private readonly IAccidents IAccidents;
        private readonly IUsers IUsers;
        public OutboundController(IShipments Shipments, IWarehouses Warehouses, 
            IPallets Pallets, ITransporters Transporters, ITransporterDrivers Drivers, 
            ITransporterTrucks Trucks, IAccidents Accidents, IUsers Users)
        {
            IShipments = Shipments;
            IWarehouses = Warehouses;
            IPallets = Pallets;
            ITransporters = Transporters;
            IDrivers = Drivers;
            ITrucks = Trucks;
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

            IEnumerable<TrxShipmentHeader> list = IShipments.GetOutboundData(warehouseId, stats, search, sortDirection, sortName);
            IEnumerable<ShipmentDTO> pagedData = Enumerable.Empty<ShipmentDTO>(); ;

            int recordsTotal = IShipments.GetTotalOutboundData(warehouseId, stats);
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

        public async Task<ActionResult> Create()
        {            
            ShipmentVM dataVM = new ShipmentVM();
            dataVM.WarehouseId = Session["warehouseAccess"].ToString();

            MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(dataVM.WarehouseId);
            dataVM.WarehouseCode = warehouse.WarehouseCode;
            dataVM.WarehouseName = warehouse.WarehouseName;

            //transporter list based on transporter access
            ViewBag.TransporterList = new SelectList(await ITransporters.GetAllByWarehouseAsync(dataVM.WarehouseId), "TransporterId", "TransporterName");
            ViewBag.DestinationList = new SelectList(await IWarehouses.GetDestinationAsync(dataVM.WarehouseId), "WarehouseId", "WarehouseName");

            return View(dataVM);
        }
        private async Task SaveValidation(ShipmentVM dataVM)
        {
            if (!string.IsNullOrEmpty(dataVM.DestinationId))
            {
                ModelState["DestinationId"].Errors.Clear();
                WarehouseValidator validator = new WarehouseValidator(IWarehouses);
                string WarehouseValid = await validator.IsWarehouseExist(dataVM.DestinationId);
                if (!WarehouseValid.Equals("true"))
                {
                    ModelState.AddModelError("DestinationId", WarehouseValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.TransporterId))
            {
                TransporterValidator validator = new TransporterValidator(ITransporters);
                string TransporterValid = await validator.IsTransporterExist(dataVM.TransporterId);
                if (!TransporterValid.Equals("true"))
                {
                    ModelState.AddModelError("TransporterId", TransporterValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.DriverId))
            {
                TransporterValidator validator = new TransporterValidator(IDrivers);
                string DriverValid = await validator.IsDriverExist(dataVM.DriverId);
                if (!DriverValid.Equals("true"))
                {
                    ModelState.AddModelError("DriverId", DriverValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.TruckId))
            {
                TransporterValidator validator = new TransporterValidator(ITrucks);
                string TruckValid = await validator.IsTruckExist(dataVM.TruckId);
                if (!TruckValid.Equals("true"))
                {
                    ModelState.AddModelError("TruckId", TruckValid);
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

            if (!string.IsNullOrEmpty(dataVM.WarehouseId) && !string.IsNullOrEmpty(dataVM.DestinationId))
            {
                ModelState["WarehouseId"].Errors.Clear();
                ModelState["DestinationId"].Errors.Clear();
                if (dataVM.WarehouseId.Equals(dataVM.DestinationId))
                {
                    ModelState.AddModelError("DestinationId", "Destination can not same with origin.");
                }
            }

            if (dataVM.PalletQty > 0)
            {
                WarehouseValidator validator = new WarehouseValidator(IWarehouses);
                string MGV = await validator.IsMGVAllowed(dataVM.PalletQty, dataVM.DestinationId);
                if (!MGV.Equals("true"))
                {
                    ModelState.AddModelError("PalletQty", MGV);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(ShipmentVM dataVM)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            bool result = false;
            string message = "Invalid form submission.";

            dataVM.WarehouseId = Session["warehouseAccess"].ToString();            

            //server validation
            await SaveValidation(dataVM);

            if (ModelState.IsValid)
            {
                MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(dataVM.WarehouseId);
                MsWarehouse destination = await IWarehouses.GetDataByIdAsync(dataVM.DestinationId);
                MsTransporter transporter = await ITransporters.GetDataByIdAsync(dataVM.TransporterId);
                MsDriver driver = await IDrivers.GetDataByIdAsync(dataVM.DriverId);
                MsTruck truck = await ITrucks.GetDataByIdAsync(dataVM.TruckId);

                TrxShipmentHeader data = new TrxShipmentHeader
                {
                    TransactionId = Utilities.CreateGuid("SHP"),
                    ShipmentNumber = dataVM.ShipmentNumber,
                    Remarks = dataVM.Remarks,
                    WarehouseId = dataVM.WarehouseId,
                    WarehouseCode = warehouse.WarehouseCode,
                    WarehouseName = warehouse.WarehouseName,
                    DestinationId = dataVM.DestinationId,
                    DestinationCode = destination.WarehouseCode,
                    DestinationName = destination.WarehouseName,
                    TransporterId = dataVM.TransporterId,
                    TransporterName = transporter.TransporterName,
                    DriverId = dataVM.DriverId,
                    DriverName = driver.DriverName,
                    TruckId = dataVM.TruckId,
                    PlateNumber = truck.PlateNumber,
                    PalletQty = dataVM.PalletQty,
                    TransactionStatus = Constant.TransactionStatus.OPEN.ToString(),
                    ShipmentStatus = Constant.ShipmentStatus.LOADING.ToString(),
                    CreatedBy = Session["username"].ToString()
                };

                result = await IShipments.CreateAsync(data);
                if (result)
                {
                    message = "Create data succeeded.";
                    TempData["TempMessage"] = message;
                    response.Add("transactionId", Utilities.EncodeTo64(Encryptor.Encrypt(data.TransactionId, Constant.facelift_encryption_key)));

                    //Task AutoDispacthShipment()
                    IEnumerable<TrxShipmentHeader> list = await IShipments.GetDataAllOutboundTransactionProgress();
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

            MsUser user = await IUsers.GetDataByIdAsync(Session["username"].ToString());

            ViewBag.Role = user.RoleId; 
            ViewBag.TransactionStatus = data.TransactionStatus;
            ViewBag.ShipmentStatus = data.ShipmentStatus;
            ViewBag.Id = x;
            ViewBag.TransporterList = new SelectList(await ITransporters.GetAllByWarehouseAsync(dataVM.WarehouseId), "TransporterId", "TransporterName");
            ViewBag.DestinationList = new SelectList(await IWarehouses.GetDestinationAsync(dataVM.WarehouseId), "WarehouseId", "WarehouseName");

            TrxAccidentHeader accidentHeader = await IAccidents.GetDataByTransactionCodeAsync(data.AccidentId);
            if(accidentHeader != null)
            {
                dataVM.shipmentBA = new ShipmentBA();
                dataVM.shipmentBA.TransactionCode = accidentHeader.TransactionCode;
                dataVM.shipmentBA.AccidentType = accidentHeader.AccidentType;
                dataVM.shipmentBA.TransactionStatus = Utilities.TransactionStatusBadge(accidentHeader.TransactionStatus);

                ViewBag.AccidentId = Utilities.EncodeTo64(Encryptor.Encrypt(accidentHeader.TransactionId, Constant.facelift_encryption_key));
                ViewBag.AccidentStatus = accidentHeader.TransactionStatus;
                ViewBag.TotalPalletAccident = accidentHeader.TrxAccidentItems.Count();
                ViewBag.TotalUnscannedPalletAccident = accidentHeader.TrxAccidentItems.Where(z => string.IsNullOrEmpty(z.ReasonType ) || string.IsNullOrEmpty(z.ReasonName)).ToList().Count();
                ViewBag.TotalScannedPalletAccident = ViewBag.TotalPalletAccident - ViewBag.TotalUnscannedPalletAccident;
            }           

            return View(dataVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Detail(string x, ShipmentVM dataVM)
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

            TrxShipmentHeader data = await IShipments.GetDataByIdAsync(id);
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

                if (!data.ShipmentStatus.Equals(Constant.ShipmentStatus.LOADING.ToString()))
                {
                    message = "Delete data not allowed, document already processed.";
                    return Json(new { stat = result, msg = message });
                }
            }

            dataVM.WarehouseId = data.WarehouseId;
            dataVM.DestinationId = data.DestinationId;

            await SaveValidation(dataVM);

            if (ModelState.IsValid)
            {
                MsWarehouse destination = await IWarehouses.GetDataByIdAsync(dataVM.DestinationId);
                MsTransporter transporter = await ITransporters.GetDataByIdAsync(dataVM.TransporterId);
                MsDriver driver = await IDrivers.GetDataByIdAsync(dataVM.DriverId);
                MsTruck truck = await ITrucks.GetDataByIdAsync(dataVM.TruckId);

                data.ShipmentNumber = dataVM.ShipmentNumber;
                data.Remarks = dataVM.Remarks;
                data.DestinationId = dataVM.DestinationId;
                data.DestinationCode = destination.WarehouseCode;
                data.DestinationName = destination.WarehouseName;
                data.TransporterId = dataVM.TransporterId;
                data.TransporterName = transporter.TransporterName;
                data.DriverId = dataVM.DriverId;
                data.DriverName = driver.DriverName;
                data.TruckId = dataVM.TruckId;
                data.PlateNumber = truck.PlateNumber;
                data.PalletQty = dataVM.PalletQty;
                
                data.ModifiedBy = Session["username"].ToString();

                result = await IShipments.UpdateAsync(data);

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

            TrxShipmentHeader data = await IShipments.GetDataByIdAsync(id);
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

                if (!data.TransactionStatus.Equals(Constant.TransactionStatus.OPEN.ToString()))
                {
                    message = "Delete document not allowed, document already processed.";
                    return Json(new { stat = result, msg = message });
                }

                if (!data.ShipmentStatus.Equals(Constant.ShipmentStatus.LOADING.ToString()))
                {
                    message = "Delete document not allowed, document already processed.";
                    return Json(new { stat = result, msg = message });
                }
            }

            data.IsDeleted = true;
            data.ModifiedBy = Session["username"].ToString();

            result = await IShipments.DeleteAsync(data);

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
        public async Task<JsonResult> Reset(string x)
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

            TrxShipmentHeader data = await IShipments.GetDataByIdAsync(id);
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
            }
             
            data.TransactionStatus = "OPEN";
            data.ShipmentStatus = "LOADING";
            data.ModifiedBy = Session["username"].ToString();

            result = await IShipments.DeleteAsync(data);

            if (result)
            {
                message = "Reset document succeeded.";
                TempData["TempMessage"] = message;
            }
            else
            {
                message = "Reset document failed.";
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

                IEnumerable<VwShipmentItem> list = IShipments.GetFilteredDataItem(transactionId, search, sortDirection, sortName);
                IEnumerable<ShipmentItemDTO> pagedData = Enumerable.Empty<ShipmentItemDTO>(); ;
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
                                    ScannedBy = x.ScannedBy,
                                    ScannedAt = Utilities.NullDateTimeToString(x.ScannedAt),
                                    DispatchedBy = x.DispatchedBy,
                                    DispatchedAt = Utilities.NullDateTimeToString(x.DispatchedAt)
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

            TrxShipmentHeader data = await IShipments.GetDataByIdAsync(id);
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

                if (!data.ShipmentStatus.Equals(Constant.ShipmentStatus.LOADING.ToString()))
                {
                    message = "Delete data not allowed, document already processed.";
                    return Json(new { stat = result, msg = message });
                }
            }

            if (selectAll)
            {
                items = data.TrxShipmentItems.Select(m => m.TransactionItemId).ToArray();
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
            result = await IShipments.DeleteItemAsync(data, items, Session["username"].ToString());

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

        public ActionResult DeliveryNote(ShipmentDTO shipment)
        {
            return View(shipment);
        }

        public async Task<ActionResult> GenerateDeliveryNoteAsync(string x)
        {
            string id = "";
            string warehouseId = Session["warehouseAccess"].ToString();
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
                        if (!data.WarehouseId.Equals(warehouseId) || data.IsDeleted)
                        {
                            throw new Exception();
                        }
                        MsUser user = await IUsers.GetDataByIdAsync(Session["username"].ToString());

                        if (!user.MsRole.RoleName.Equals(Constant.RoleName.Sysadmin.ToString()))
                        {
                            if (!data.ShipmentStatus.Equals(Constant.ShipmentStatus.LOADING.ToString()))
                            {
                                throw new Exception();
                            }
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
                return Content(@"<body>
                       <script type='text/javascript'>
                         window.close();
                       </script>
                     </body> ");
            }
            ShipmentDTO shipment = new ShipmentDTO();
            shipment.TransactionCode = data.TransactionCode;
            shipment.CreatedAt = Utilities.NullDateToString(data.CreatedAt);
            shipment.WarehouseCode = data.WarehouseCode;
            shipment.WarehouseName = data.WarehouseName;
            shipment.DestinationCode = data.DestinationCode;
            shipment.DestinationName = data.DestinationName;
            shipment.TransporterName = data.TransporterName;
            shipment.DriverName = data.DriverName;
            shipment.PlateNumber = data.PlateNumber;
            shipment.PalletQty = data.PalletQty.ToString();
            shipment.ShipmentNumber = !string.IsNullOrEmpty(data.ShipmentNumber) ? data.ShipmentNumber : "-";
            shipment.Remarks = !string.IsNullOrEmpty(data.Remarks) ? data.Remarks : "-";
            shipment.CreatedBy = data.CreatedBy;

            string Domain = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');

            ViewBag.Logo = Domain + "/Content/img/logo_black.png";
            ViewBag.QrCode = Domain + "/" + GenerateQRCode(data.TransactionId, data.TransactionCode);
            String body = Utilities.RenderViewToString(this.ControllerContext, "DeliveryNote", shipment);
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                using (var pdfWriter = new PdfWriter(stream))
                {
                    HtmlConverter.ConvertToPdf(body, pdfWriter);
                    byte[] file = stream.ToArray();
                    MemoryStream output = new MemoryStream();
                    output.Write(file, 0, file.Length);
                    output.Position = 0;

                    Response.AddHeader("content-disposition", "inline; filename=form.pdf");
                    // Return the output stream
                    return File(output, "application/pdf");
                }                
            }
        }

        private string GenerateQRCode(string id, string value)
        {
            string folderPath = "/Content/img/DeliveryQRCode";
            string imagePath = folderPath + "/" + string.Format("{0}.png", id);
            // If the directory doesn't exist then create it.
            if (!Directory.Exists(Server.MapPath("~" + folderPath)))
            {
                Directory.CreateDirectory(Server.MapPath("~" + folderPath));
            }

            var barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            barcodeWriter.Options = new QrCodeEncodingOptions
            {
                Width = 300,
                Height = 300
            };
            var result = barcodeWriter.Write(value);

            string barcodePath = Server.MapPath("~" + imagePath);
            var barcodeBitmap = new Bitmap(result);
            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream fs = new FileStream(barcodePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    barcodeBitmap.Save(memory, ImageFormat.Png);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            //string Domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            return imagePath;
        }

        [HttpPost]
        public async Task<JsonResult> ApproveBA(string x)
        {
            bool result = false;
            string message = "";
            string id = "";
            string warehouseId = Session["warehouseAccess"].ToString();
            MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(warehouseId);
            MsUser user = await IUsers.GetDataByIdAsync(Session["username"].ToString());

            if (!user.Username.Equals(warehouse.PIC1) || !user.Username.Equals(warehouse.PIC2))
            {
                message = "User not authorized to approve BA.";
                return Json(new { stat = result, msg = message });
            }

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
                message = "Approve document failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            TrxAccidentHeader data = await IAccidents.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }
            else
            {
                if (data.IsDeleted)
                {
                    message = "Bad Request. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }
                //check previous data, close can be executed if status == on progress

                if (data.TrxAccidentItems.Count() < 1 && !data.TransactionStatus.Equals(Constant.TransactionStatus.PROGRESS.ToString()))
                {
                    message = "Document not allowed to be approved.";
                    return Json(new { stat = result, msg = message });
                }

                int totalUnscanned = data.TrxAccidentItems.Where(z => string.IsNullOrEmpty(z.ReasonType) || string.IsNullOrEmpty(z.ReasonName)).ToList().Count();
                if (totalUnscanned > 0)
                {
                    message = "Document not allowed to be approved. Please update all pallet reason.";
                    return Json(new { stat = result, msg = message });
                }

                if (data.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                {
                    message = "Approve document not allowed, document already closed.";
                    return Json(new { stat = result, msg = message });
                }
            }

            data.TransactionStatus = Constant.TransactionStatus.CLOSED.ToString();
            data.ApprovedBy = Session["username"].ToString();

            result = await IAccidents.CloseAsync(data);
            //after close, insert pallet to master pallet
            if (result)
            {
                message = "Approve data succeeded.";
                TempData["TempMessage"] = message;
            }
            else
            {
                message = "Approve document failed. Please contact system administrator.";
            }

            return Json(new { stat = result, msg = message });
        }


        [HttpPost]
        public async Task<JsonResult> Dispatch(string x)
        {
            bool status = false;
            bool result = false;
            string message = "";
            string id = "";
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

            //if 1 or more item uploaded, transaction status will be changed as progress
            if (header.TrxShipmentItems != null && header.TrxShipmentItems.Count() > 0)
            {
                header.TransactionStatus = Constant.TransactionStatus.PROGRESS.ToString();
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

            string username = "System";
            status = await IShipments.DispatchItemAsync(header, username, "Dispatch Item (Scan)");

            if (status)
            {
                message = "Pallet dispatch successfuly.";

                //Task AutoDispacthShipment()
                IEnumerable<TrxShipmentHeader> list = await IShipments.GetDataAllOutboundTransactionProgress();
            }
            else
            {
                message = "Pallet dispatch failed. Please contact system administrator.";
            }

            return Json(new { stat = result, msg = message });
        }

        public ActionResult ExportListToExcel()
        {
            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_Outbound_{0}.xlsx", date);
            string warehouseId = Session["warehouseAccess"].ToString();
            IEnumerable<TrxShipmentHeader> list = IShipments.GetAllOutboundTransactions(warehouseId);
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
            workSheet.Cells[1, 6].Value = "Destination Code";
            workSheet.Cells[1, 7].Value = "Destination Name";
            workSheet.Cells[1, 8].Value = "Transporter Name";
            workSheet.Cells[1, 9].Value = "Driver Name";
            workSheet.Cells[1, 10].Value = "Plate Number";
            workSheet.Cells[1, 11].Value = "Transaction Status";
            workSheet.Cells[1, 12].Value = "Shipment Status";
            workSheet.Cells[1, 13].Value = "Created By";
            workSheet.Cells[1, 14].Value = "Created At";
            workSheet.Cells[1, 15].Value = "Modified By";
            workSheet.Cells[1, 16].Value = "Modified At";
            workSheet.Cells[1, 17].Value = "Approved By";
            workSheet.Cells[1, 18].Value = "Approved At";

            int recordIndex = 2;
            foreach (TrxShipmentHeader header in list)
            {
                workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
                workSheet.Cells[recordIndex, 2].Value = header.ShipmentNumber;
                workSheet.Cells[recordIndex, 3].Value = header.Remarks;
                workSheet.Cells[recordIndex, 4].Value = header.WarehouseCode;
                workSheet.Cells[recordIndex, 5].Value = header.WarehouseName;
                workSheet.Cells[recordIndex, 6].Value = header.DestinationCode;
                workSheet.Cells[recordIndex, 7].Value = header.DestinationName;
                workSheet.Cells[recordIndex, 8].Value = header.TransporterName;
                workSheet.Cells[recordIndex, 9].Value = header.DriverName;
                workSheet.Cells[recordIndex, 10].Value = header.PlateNumber;
                workSheet.Cells[recordIndex, 11].Value = header.TransactionStatus;
                workSheet.Cells[recordIndex, 12].Value = header.ShipmentStatus;
                workSheet.Cells[recordIndex, 13].Value = header.CreatedBy;
                workSheet.Cells[recordIndex, 14].Value = Utilities.NullDateTimeToString(header.CreatedAt);
                workSheet.Cells[recordIndex, 15].Value = header.ModifiedBy;
                workSheet.Cells[recordIndex, 16].Value = Utilities.NullDateTimeToString(header.ModifiedAt);
                workSheet.Cells[recordIndex, 17].Value = header.ApprovedBy;
                workSheet.Cells[recordIndex, 18].Value = Utilities.NullDateTimeToString(header.ApprovedAt);
                recordIndex++;
            }

            for (int i = 1; i <= 18; i++)
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
            String fileName = String.Format("filename=Facelift_Outbound_Details_{0}.xlsx", date);
            string warehouseId = Session["warehouseAccess"].ToString();
            IEnumerable<TrxShipmentHeader> list = IShipments.GetAllOutboundTransactions(warehouseId);
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
            workSheet.Cells[1, 6].Value = "Destination Code";
            workSheet.Cells[1, 7].Value = "Destination Name";
            workSheet.Cells[1, 8].Value = "Transporter Name";
            workSheet.Cells[1, 9].Value = "Driver Name";
            workSheet.Cells[1, 10].Value = "Plate Number";
            workSheet.Cells[1, 11].Value = "Transaction Status";
            workSheet.Cells[1, 12].Value = "Shipment Status";
            workSheet.Cells[1, 13].Value = "Created By";
            workSheet.Cells[1, 14].Value = "Created At";
            workSheet.Cells[1, 15].Value = "Modified By";
            workSheet.Cells[1, 16].Value = "Modified At";
            workSheet.Cells[1, 17].Value = "Approved By";
            workSheet.Cells[1, 18].Value = "Approved At";
            workSheet.Cells[1, 19].Value = "Tag Id";
            workSheet.Cells[1, 20].Value = "Scanned By";
            workSheet.Cells[1, 21].Value = "Scanned At";
            workSheet.Cells[1, 22].Value = "Dispatched By";
            workSheet.Cells[1, 23].Value = "Dispatched At";
            workSheet.Cells[1, 24].Value = "Received By";
            workSheet.Cells[1, 25].Value = "Received At";
            
            int recordIndex = 2;
            foreach (TrxShipmentHeader header in list)
            {
                foreach (TrxShipmentItem item in header.TrxShipmentItems)
                {
                    workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
                    workSheet.Cells[recordIndex, 2].Value = header.ShipmentNumber;
                    workSheet.Cells[recordIndex, 3].Value = header.Remarks;
                    workSheet.Cells[recordIndex, 4].Value = header.WarehouseCode;
                    workSheet.Cells[recordIndex, 5].Value = header.WarehouseName;
                    workSheet.Cells[recordIndex, 6].Value = header.DestinationCode;
                    workSheet.Cells[recordIndex, 7].Value = header.DestinationName;
                    workSheet.Cells[recordIndex, 8].Value = header.TransporterName;
                    workSheet.Cells[recordIndex, 9].Value = header.DriverName;
                    workSheet.Cells[recordIndex, 10].Value = header.PlateNumber;
                    workSheet.Cells[recordIndex, 11].Value = header.TransactionStatus;
                    workSheet.Cells[recordIndex, 12].Value = header.ShipmentStatus;
                    workSheet.Cells[recordIndex, 13].Value = header.CreatedBy;
                    workSheet.Cells[recordIndex, 14].Value = Utilities.NullDateTimeToString(header.CreatedAt);
                    workSheet.Cells[recordIndex, 15].Value = header.ModifiedBy;
                    workSheet.Cells[recordIndex, 16].Value = header.ModifiedAt;
                    workSheet.Cells[recordIndex, 17].Value = header.ApprovedBy;
                    workSheet.Cells[recordIndex, 18].Value = Utilities.NullDateTimeToString(header.ApprovedAt);
                    workSheet.Cells[recordIndex, 19].Value = item.TagId;
                    workSheet.Cells[recordIndex, 20].Value = item.ScannedBy;
                    workSheet.Cells[recordIndex, 21].Value = Utilities.NullDateTimeToString(item.ScannedAt);
                    workSheet.Cells[recordIndex, 22].Value = item.DispatchedBy;
                    workSheet.Cells[recordIndex, 23].Value = Utilities.NullDateTimeToString(item.DispatchedAt);
                    workSheet.Cells[recordIndex, 24].Value = item.ReceivedBy;
                    workSheet.Cells[recordIndex, 25].Value = Utilities.NullDateTimeToString(item.ReceivedAt);
                    recordIndex++;
                }
            }

            for (int i = 1; i <= 25; i++)
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

        [HttpGet]
        public async Task<JsonResult> GetDriverByTransporterId(string transporterId)
        {
            bool result = false;
            string message = "";
            try
            {
                if (string.IsNullOrEmpty(transporterId))
                {
                    throw new Exception();
                }

            }
            catch (Exception)
            {
                message = "Id not found. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            MsTransporter data = await ITransporters.GetDataByIdAsync(transporterId);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }

            IEnumerable<MsDriver> list = await IDrivers.GetAllAsync(data.TransporterId);
            IEnumerable<TransporterDriverDTO> drivers = from x in list
                                                        select new TransporterDriverDTO
                                                        {
                                                            DriverId = x.DriverId,
                                                            DriverName = x.DriverName
                                                        };
            result = true;
            message = "Data found.";

            return Json(new { stat = result, msg = message, list = drivers }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetTruckByTransporterId(string transporterId)
        {
            bool result = false;
            string message = "";
            try
            {
                if (string.IsNullOrEmpty(transporterId))
                {
                    throw new Exception();
                }

            }
            catch (Exception)
            {
                message = "Id not found. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            MsTransporter data = await ITransporters.GetDataByIdAsync(transporterId);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }

            IEnumerable<MsTruck> list = await ITrucks.GetAllAsync(data.TransporterId);
            IEnumerable<TransporterTruckDTO> trucks = from x in list
                                                      select new TransporterTruckDTO
                                                      {
                                                          TruckId = x.TruckId,
                                                          PlateNumber = x.PlateNumber
                                                      };
            result = true;
            message = "Data found.";

            return Json(new { stat = result, msg = message, list = trucks }, JsonRequestBehavior.AllowGet);
        }
    }
}