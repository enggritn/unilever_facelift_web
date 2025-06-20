using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Facelift_App.Validators;
using System.Web.Mvc;
using Facelift_App.Models;
using Facelift_App.Helper;
using Facelift_App.Services;

namespace Facelift_App.Controllers
{
    [SessionCheck]
    [AuthCheck]
    public class TransporterController : Controller
    {
        private readonly ITransporters ITransporters;
        private readonly ITransporterDrivers ITransporterDrivers;
        private readonly ITransporterTrucks ITransporterTrucks;
        private readonly IWarehouses IWarehouses;


        public TransporterController(ITransporters Transporters, ITransporterDrivers TransporterDrivers, ITransporterTrucks TransporterTrucks, IWarehouses Warehouses)
        {
            ITransporters = Transporters;
            ITransporterDrivers = TransporterDrivers;
            ITransporterTrucks = TransporterTrucks;
            IWarehouses = Warehouses;

        }

        // GET: Transporters
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Create()
        {
            ViewBag.WarehouseList = await IWarehouses.GetAllAsync();
            return View();
        }

        private async Task CreateValidation(TransporterVM dataVM)
        {
            if (!string.IsNullOrEmpty(dataVM.TransporterName))
            {
                TransporterValidator validator = new TransporterValidator(ITransporters);
                string TransporterValid = await validator.IsUniqueName(dataVM.TransporterName, "");
                if (!TransporterValid.Equals("true"))
                {
                    ModelState.AddModelError("TransporterName", TransporterValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(TransporterVM dataVM)
        {
            //ModelState["MemberName"].Errors.Clear();
            bool result = false;
            string message = "Invalid form submission.";

            //server validation
            await CreateValidation(dataVM);

            if (ModelState.IsValid)
            {
                dataVM.TransporterName = Utilities.ToUpper(dataVM.TransporterName);
                dataVM.Email = Utilities.ToLower(dataVM.Email);
                dataVM.Address = !string.IsNullOrEmpty(dataVM.Address) ? dataVM.Address.Trim() : "";
                dataVM.PIC = Utilities.UpperFirstCase(dataVM.PIC);


                MsTransporter data = new MsTransporter
                {
                    TransporterId = Utilities.CreateGuid("T"),
                    TransporterName = dataVM.TransporterName,
                    Address = dataVM.Address,
                    Phone = dataVM.Phone,
                    Email = dataVM.Email,
                    PIC = dataVM.PIC,
                    IsActive = true,
                    CreatedBy = Session["username"].ToString()
                };

                data.MsTransporterAccesses = new List<MsTransporterAccess>();
                if (dataVM.WarehouseIds != null && dataVM.WarehouseIds.Count() > 0)
                {
                    foreach (string warehouseId in dataVM.WarehouseIds)
                    {
                        MsTransporterAccess access = new MsTransporterAccess
                        {
                            TransporterId = data.TransporterId,
                            WarehouseId = warehouseId
                        };

                        data.MsTransporterAccesses.Add(access);
                    }
                }


                result = await ITransporters.InsertAsync(data);
                if (result)
                {
                    message = "Create data succeeded.";
                }
                else
                {
                    message = "Create data failed. Please contact system administrator.";
                }

            }

            return Json(new { stat = result, msg = message });
        }


        public async Task<ActionResult> Detail(string x)
        {
            string id = "";
            MsTransporter data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = await ITransporters.GetDataByIdAsync(id);
                    if (data == null)
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Create");
            }

            TransporterVM dataVM = new TransporterVM
            {
                TransporterName = data.TransporterName,
                Address = data.Address,
                PIC = data.PIC,
                Email = data.Email,
                Phone = data.Phone,
                IsActive = data.IsActive
            };

            dataVM.WarehouseIds = data.MsTransporterAccesses.Select(m => m.WarehouseId).ToArray();

            IEnumerable<MsWarehouse> warehouses = await IWarehouses.GetAllAsync();
            ViewBag.SelectedWarehouseList = warehouses.Where(m => dataVM.WarehouseIds.Contains(m.WarehouseId));
            ViewBag.UnSelectedWarehouseList = warehouses.Where(m => !dataVM.WarehouseIds.Contains(m.WarehouseId));


            ViewBag.Id = x;
            return View(dataVM);
        }

        private async Task DetailValidation(TransporterVM dataVM, string id)
        {
            if (!string.IsNullOrEmpty(dataVM.TransporterName))
            {
                TransporterValidator validator = new TransporterValidator(ITransporters);
                string TransporterValid = await validator.IsUniqueName(dataVM.TransporterName, id);
                if (!TransporterValid.Equals("true"))
                {
                    ModelState.AddModelError("TransporterName", TransporterValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Detail(string x, TransporterVM dataVM)
        {
            bool result = false;
            string message = "Invalid form submission.";
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
                message = "Update data failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            MsTransporter data = await ITransporters.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }

            await DetailValidation(dataVM, id);

            if (ModelState.IsValid)
            {
                dataVM.TransporterName = Utilities.ToUpper(dataVM.TransporterName);
                dataVM.Email = Utilities.ToLower(dataVM.Email);
                dataVM.Address = !string.IsNullOrEmpty(dataVM.Address) ? dataVM.Address.Trim() : "";
                dataVM.PIC = Utilities.UpperFirstCase(dataVM.PIC);

                data.TransporterName = dataVM.TransporterName;
                data.Address = dataVM.Address;
                data.Phone = dataVM.Phone;
                data.Email = dataVM.Email;
                data.PIC = dataVM.PIC;
                data.IsActive = dataVM.IsActive;
                data.ModifiedBy = Session["username"].ToString();

                List<MsTransporterAccess> prevAccess = data.MsTransporterAccesses.ToList();
                string[] PrevWarehouseIds = prevAccess.Select(m => m.WarehouseId).ToArray();

                List<MsTransporterAccess> warehouseAccesses = new List<MsTransporterAccess>();
                if (dataVM.WarehouseIds != null && dataVM.WarehouseIds.Count() > 0)
                {
                    foreach (string warehouseId in dataVM.WarehouseIds)
                    {
                        MsTransporterAccess access = new MsTransporterAccess();
                        if (!PrevWarehouseIds.Contains(warehouseId))
                        {
                            access.TransporterId = data.TransporterId;
                            access.WarehouseId = warehouseId;
                        }
                        else
                        {
                            access = prevAccess.Where(m => m.WarehouseId.Equals(warehouseId)).FirstOrDefault();
                        }
                        warehouseAccesses.Add(access);
                    }
                }

                data.MsTransporterAccesses = warehouseAccesses.ToList();


                result = await ITransporters.UpdateAsync(data);

                if (result)
                {
                    message = "Update data succeeded.";
                }
                else
                {
                    message = "Update data failed. Please contact system administrator.";
                }
            }

            return Json(new { stat = result, msg = message });
        }

        [HttpPost]
        public ActionResult Datatable()
        {
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            IEnumerable<MsTransporter> list = ITransporters.GetFilteredData(search, sortDirection, sortName);
            IEnumerable<TransporterDTO> pagedData = Enumerable.Empty<TransporterDTO>(); ;

            int recordsTotal = ITransporters.GetTotalData();
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new TransporterDTO
                            {
                                TransporterId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransporterId, Constant.facelift_encryption_key)),
                                TransporterName = x.TransporterName,
                                Address = !string.IsNullOrEmpty(x.Address) ? x.Address : "-",
                                Email = !string.IsNullOrEmpty(x.Email) ? x.Email : "-",
                                Phone = !string.IsNullOrEmpty(x.Phone) ? x.Phone : "-",
                                PIC = !string.IsNullOrEmpty(x.PIC) ? x.PIC : "-",
                                IsActive = Utilities.IsActiveStatusBadge(x.IsActive),
                                CreatedBy = x.CreatedBy,
                                CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
                                ModifiedBy = !string.IsNullOrEmpty(x.ModifiedBy) ? x.ModifiedBy : "-",
                                ModifiedAt = Utilities.NullDateTimeToString(x.ModifiedAt)
                            };
            }


            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }


        private async Task SaveDriverValidation(TransporterDriverVM dataVM)
        {
            ModelState["TransporterName"].Errors.Clear();
            ModelState["Phone"].Errors.Clear();
            ModelState["Email"].Errors.Clear();
            ModelState["PIC"].Errors.Clear();

            if (!string.IsNullOrEmpty(dataVM.LicenseNumber))
            {
                TransporterValidator validator = new TransporterValidator(ITransporterDrivers);
                string TransporterValid = await validator.IsUniqueLicense(dataVM.LicenseNumber, dataVM.DriverId);
                if (!TransporterValid.Equals("true"))
                {
                    ModelState.AddModelError("LicenseNumber", TransporterValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveDriver(string x, TransporterVM dataVM)
        {
            //x == transporter header id
            bool result = false;
            string message = "Invalid form submission.";
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
                message = "Update data failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            MsTransporter transporter = await ITransporters.GetDataByIdAsync(id);
            if (transporter == null)
            {
                message = "Transporter Data not found.";
                return Json(new { stat = result, msg = message });
            }

            TransporterDriverVM transporterDriverVM = dataVM.transporterDriverVM;

            if (!string.IsNullOrEmpty(transporterDriverVM.DriverId))
            {
                transporterDriverVM.DriverId = Encryptor.Decrypt(Utilities.DecodeFrom64(transporterDriverVM.DriverId), Constant.facelift_encryption_key);
            }
            

            await SaveDriverValidation(transporterDriverVM);

            if (ModelState.IsValid)
            {
                MsDriver driver = new MsDriver();
                driver.TransporterId = id;
                driver.DriverId = transporterDriverVM.DriverId;
                driver.DriverName = Utilities.UpperFirstCase(transporterDriverVM.DriverName);
                driver.Phone = transporterDriverVM.Phone;
                driver.LicenseNumber = transporterDriverVM.LicenseNumber;
                driver.IsActive = transporterDriverVM.IsActive;

                MsDriver data = null;
                if (!string.IsNullOrEmpty(driver.DriverId))
                {
                    data = await ITransporterDrivers.GetDataByIdAsync(driver.DriverId);
                }
                   
                if (data == null)
                {
                    driver.DriverId = Utilities.CreateGuid("Tdr");
                    driver.IsActive = true;
                    driver.CreatedBy = Session["username"].ToString();

                    data = driver;

                    result = await ITransporterDrivers.InsertAsync(data);
                    if (result)
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
                    data.DriverName = driver.DriverName;
                    data.Phone = driver.Phone;
                    data.LicenseNumber = driver.LicenseNumber;
                    data.IsActive = driver.IsActive;
                    data.ModifiedBy = Session["username"].ToString();

                    result = await ITransporterDrivers.UpdateAsync(data);
                    if (result)
                    {
                        message = "Update data succeeded.";
                    }
                    else
                    {
                        message = "Update data failed. Please contact system administrator.";
                    }
                }
            }
            
            return Json(new { stat = result, msg = message });
        }


        [HttpGet]
        public async Task<ActionResult> GetDriverDetail(string x)
        {
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
                message = "Id not found. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            MsDriver data = await ITransporterDrivers.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }

            TransporterDriverVM transporterDriverVM = new TransporterDriverVM
            {
                DriverId = x,
                DriverName = data.DriverName,
                Phone = data.Phone,
                LicenseNumber = data.LicenseNumber,
                IsActive = data.IsActive
            };

            result = true;
            message = "Data found.";

            return Json(new { stat = result, msg = message, data = transporterDriverVM }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DatatableDriver(string id)
        {
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            string TransporterId = Encryptor.Decrypt(Utilities.DecodeFrom64(id), Constant.facelift_encryption_key);

            IEnumerable<MsDriver> list = ITransporterDrivers.GetFilteredData(TransporterId, search, sortDirection, sortName);
            IEnumerable<TransporterDriverDTO> pagedData = Enumerable.Empty<TransporterDriverDTO>(); ;

            int recordsTotal = ITransporterDrivers.GetTotalData(TransporterId);
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new TransporterDriverDTO
                            {
                                DriverId = Utilities.EncodeTo64(Encryptor.Encrypt(x.DriverId, Constant.facelift_encryption_key)),
                                TransporterId = x.TransporterId,
                                DriverName = x.DriverName,
                                Phone = x.Phone,
                                LicenseNumber = x.LicenseNumber,
                                IsActive = x.IsActive,
                                CreatedBy = x.CreatedBy,
                                CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
                                ModifiedBy = !string.IsNullOrEmpty(x.ModifiedBy) ? x.ModifiedBy : "-",
                                ModifiedAt = Utilities.NullDateTimeToString(x.ModifiedAt)
                            };
            }


            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }


        private async Task SaveTruckValidation(TransporterTruckVM dataVM)
        {
            ModelState["TransporterName"].Errors.Clear();
            ModelState["Phone"].Errors.Clear();
            ModelState["Email"].Errors.Clear();
            ModelState["PIC"].Errors.Clear();

            if (!string.IsNullOrEmpty(dataVM.PlateNumber))
            {
                TransporterValidator validator = new TransporterValidator(ITransporterTrucks);
                string TransporterValid = await validator.IsUniquePlate(dataVM.PlateNumber, dataVM.TruckId);
                if (!TransporterValid.Equals("true"))
                {
                    ModelState.AddModelError("PlateNumber", TransporterValid);
                }
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveTruck(string x, TransporterVM dataVM)
        {
            //x == transporter header id
            bool result = false;
            string message = "Invalid form submission.";
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
                message = "Update data failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            MsTransporter transporter = await ITransporters.GetDataByIdAsync(id);
            if (transporter == null)
            {
                message = "Transporter Data not found.";
                return Json(new { stat = result, msg = message });
            }


            TransporterTruckVM transporterTruckVM = dataVM.transporterTruckVM;


            if (!string.IsNullOrEmpty(transporterTruckVM.TruckId))
            {
                transporterTruckVM.TruckId = Encryptor.Decrypt(Utilities.DecodeFrom64(transporterTruckVM.TruckId), Constant.facelift_encryption_key);
            }


            await SaveTruckValidation(transporterTruckVM);

            if (ModelState.IsValid)
            {
                MsTruck truck = new MsTruck();
                truck.TransporterId = id;
                truck.TruckId = dataVM.transporterTruckVM.TruckId;
                truck.PlateNumber = Utilities.ToUpper(dataVM.transporterTruckVM.PlateNumber);
                truck.IsActive = dataVM.transporterTruckVM.IsActive;

                MsTruck data = null;
                if (!string.IsNullOrEmpty(truck.TruckId))
                {
                    data = await ITransporterTrucks.GetDataByIdAsync(truck.TruckId);
                }

                if (data == null)
                {
                    truck.TruckId = Utilities.CreateGuid("Ttr");
                    truck.IsActive = true;
                    truck.CreatedBy = Session["username"].ToString();

                    data = truck;

                    result = await ITransporterTrucks.InsertAsync(data);
                    if (result)
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
                    data.PlateNumber = truck.PlateNumber;
                    data.IsActive = truck.IsActive;
                    data.ModifiedBy = Session["username"].ToString();

                    result = await ITransporterTrucks.UpdateAsync(data);
                    if (result)
                    {
                        message = "Update data succeeded.";
                    }
                    else
                    {
                        message = "Update data failed. Please contact system administrator.";
                    }
                }
            }

            return Json(new { stat = result, msg = message });
        }

        [HttpGet]
        public async Task<ActionResult> GetTruckDetail(string x)
        {
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
                message = "Id not found. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            MsTruck data = await ITransporterTrucks.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }

            TransporterTruckVM transporterTruckVM = new TransporterTruckVM
            {
                TruckId = x,
                PlateNumber = data.PlateNumber,
                IsActive = data.IsActive
            };

            result = true;
            message = "Data found.";

            return Json(new { stat = result, msg = message, data = transporterTruckVM }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DatatableTruck(string id)
        {
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            string TransporterId = Encryptor.Decrypt(Utilities.DecodeFrom64(id), Constant.facelift_encryption_key);

            IEnumerable<MsTruck> list = ITransporterTrucks.GetFilteredData(TransporterId, search, sortDirection, sortName);
            IEnumerable<TransporterTruckDTO> pagedData = Enumerable.Empty<TransporterTruckDTO>(); ;

            int recordsTotal = ITransporterTrucks.GetTotalData(TransporterId);
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new TransporterTruckDTO
                            {
                                TruckId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TruckId, Constant.facelift_encryption_key)),
                                TransporterId = x.TransporterId,
                                PlateNumber = x.PlateNumber,
                                IsActive = x.IsActive,
                                CreatedBy = x.CreatedBy,
                                CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
                                ModifiedBy = !string.IsNullOrEmpty(x.ModifiedBy) ? x.ModifiedBy : "-",
                                ModifiedAt = Utilities.NullDateTimeToString(x.ModifiedAt)
                            };
            }


            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }
    }
}
