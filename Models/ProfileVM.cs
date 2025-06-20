using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace Facelift_App.Models
{
    public class ProfileVM
    {
        public string Username { get; set; }
        public string UserEmail { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public string LastVisitUrl { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime? LastVisitAt { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime? LastLoginAt { get; set; }
        public string CreatedBy { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime? CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime? ModifiedAt { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime? ChPassAt { get; set; }

        [Required(ErrorMessage = "Current Password is required.")]
        [Remote("IsPasswordMatch", "UserValidation")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        [MinLength(5, ErrorMessage = "New Password can not less than 5 characters.")]
        [MaxLength(20, ErrorMessage = "New Password can not more than 20 characters.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Password Confirmation is required.")]
        [Compare("NewPassword", ErrorMessage = "Password Confirmation not match.")]
        [DataType(DataType.Password)]
        public string PasswordConfirmation { get; set; }

        [Remote("IsWarehouseExist", "WarehouseValidation")]
        public string WarehouseId { get; set; }


    }
}