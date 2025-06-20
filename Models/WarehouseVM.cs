using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace Facelift_App.Models
{
    public class WarehouseVM
    {

        public string WarehouseCode { get; set; }

        [Required(ErrorMessage = "Warehouse Name is required.")]
        [MinLength(2, ErrorMessage = "Warehouse Name can not less than 2 characters.")]
        [MaxLength(50, ErrorMessage = "Warehouse Name can not more than 50 characters.")]
        [Remote("IsUniqueName", "WarehouseValidation", AdditionalFields = "x")]
        public string WarehouseName { get; set; }

        [Required(ErrorMessage = "Warehouse Alias is required.")]
        [MinLength(2, ErrorMessage = "Warehouse Alias can not less than 2 characters.")]
        [MaxLength(4, ErrorMessage = "Warehouse Alias can not more than 4 characters.")]
        [Remote("IsUniqueAlias", "WarehouseValidation", AdditionalFields = "x")]
        public string WarehouseAlias { get; set; }

        public string Address { get; set; }

        [MaxLength(20, ErrorMessage = "Phone can not more than 20 characters.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Phone is not valid.")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        public string PIC1 { get; set; }

        public string PIC2 { get; set; }

        //[Required(ErrorMessage = "Fix Capacity is required.")]
        //[Range(0, int.MaxValue, ErrorMessage = "Fix Capacity can not below 0.")]
        //[Remote("IsWeightAllowed", "WarehouseValidation", AdditionalFields = "x")]
        public int MaxCapacity { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [Remote("IsExist", "WarehouseCategoryValidation")]
        public string CategoryId { get; set; }


        public IEnumerable<MsCompany> CompanyList { get; set; }


        [MustHaveOneElementAttribute(ErrorMessage = "Please choose billable status.")]
        public string[] CompanyIds { get; set; }


        public class MustHaveOneElementAttribute : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                var list = value as IList;
                if (list != null)
                {
                    return list.Count > 0;
                }
                return false;
            }
        }
    }
}