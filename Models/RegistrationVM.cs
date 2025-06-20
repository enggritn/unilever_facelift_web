using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Facelift_App.Models
{
    public class RegistrationVM
    {
        public string TransactionCode { get; set; }
        public string DeliveryNote { get; set; }
        public string Description { get; set; }
        [Required(ErrorMessage = "Pallet Type is required.")]
        [Remote("IsTypeExist", "PalletTypeValidation")]
        public string PalletTypeId { get; set; }
        public string PalletTypeName { get; set; }
        [Required(ErrorMessage = "Warehouse is required.")]
        [Remote("IsWarehouseExist", "WarehouseValidation")]
        public string WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        [Required(ErrorMessage = "Owner is required.")]
        [Remote("IsExist", "CompanyValidation")]
        public string CompanyId { get; set; }
        public string PalletOwner { get; set; }

        [Required(ErrorMessage = "Producer is required.")]
        [Remote("IsExist", "ProducerValidation")]
        public string ProducerName { get; set; }
        public string PalletProducer { get; set; }
        [Required(ErrorMessage = "Produced Date is required.")]
        [ValidProducedDate]
        public DateTime ProducedDate { get; set; }
        public string TransactionStatus { get; set; }
        public string CreatedBy { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime? ModifiedAt { get; set; }
        public string ApprovedBy { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime? ApprovedAt { get; set; }

        public List<LogRegistrationHeader> logs { get; set; }
        public List<LogRegistrationDocument> versions { get; set; }
    }


    public class ValidProducedDate : ValidationAttribute
    {
        protected override ValidationResult
                IsValid(object value, ValidationContext validationContext)
        {
            DateTime ProducedDate = Convert.ToDateTime(value);
            if (ProducedDate > DateTime.Now)
            {
                return new ValidationResult
                    ("Production date can not be greater than current date.");
                
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }
}