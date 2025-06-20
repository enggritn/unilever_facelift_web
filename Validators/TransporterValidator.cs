using Facelift_App.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Facelift_App.Validators
{
    public class TransporterValidator
    {
        private readonly ITransporters ITransporters;
        private readonly ITransporterDrivers ITransporterDrivers;
        private readonly ITransporterTrucks ITransporterTrucks;

        public TransporterValidator(ITransporters Transporters, ITransporterDrivers TransporterDrivers, ITransporterTrucks TransporterTrucks)
        {
            ITransporters = Transporters;
            ITransporterDrivers = TransporterDrivers;
            ITransporterTrucks = TransporterTrucks;
        }

        public TransporterValidator(ITransporters Transporters)
        {
            ITransporters = Transporters;
        }

        public TransporterValidator(ITransporterDrivers TransporterDrivers)
        {
            ITransporterDrivers = TransporterDrivers;
        }

        public TransporterValidator(ITransporterTrucks TransporterTrucks)
        {
            ITransporterTrucks = TransporterTrucks;
        }

        public async Task<string> IsUniqueName(string TransporterName, string id)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            string errMsg = "true";
            MsTransporter data = await ITransporters.GetDataByTransporterNameAsync(TransporterName);
            if (data != null && !data.TransporterId.Equals(id))
            {
                errMsg = "Transporter Name already registered.";
            }
            return errMsg;
        }

        public async Task<string> IsUniqueLicense(string LicenseNumber, string DriverId)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            string errMsg = "true";
            MsDriver data = await ITransporterDrivers.GetDataByLicenseNumberAsync(LicenseNumber);
            if (data != null && !data.DriverId.Equals(DriverId))
            {
                errMsg = "License Number already registered.";
            }
            return errMsg;
        }

        public async Task<string> IsUniquePlate(string PlateNumber, string TruckId)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            string errMsg = "true";
            MsTruck data = await ITransporterTrucks.GetDataByPlateNumberAsync(PlateNumber);
            if (data != null && !data.TruckId.Equals(TruckId))
            {
                errMsg = "Plate Number already registered.";
            }
            return errMsg;
        }

        public async Task<string> IsTransporterExist(string TransporterId)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            return await ITransporters.GetDataByIdAsync(TransporterId) != null ? "true" : "Transporter not recognized.";
        }

        public async Task<string> IsDriverExist(string DriverId)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            return await ITransporterDrivers.GetDataByIdAsync(DriverId) != null ? "true" : "Driver not recognized.";
        }

        public async Task<string> IsTruckExist(string TruckId)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            return await ITransporterTrucks.GetDataByIdAsync(TruckId) != null ? "true" : "Truck not recognized.";
        }
    }
}