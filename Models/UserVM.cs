using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace Facelift_App.Models
{
    public class UserVM
    {
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(5, ErrorMessage = "Username can not less than 5 characters.")]
        [MaxLength(30, ErrorMessage = "Username can not more than 30 characters.")]
        //[RegularExpression("^[a-z]+$", ErrorMessage = "Username is not valid.")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Username is not valid.")]
        [Remote("IsUsernameUnique", "UserValidation")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        [MaxLength(50, ErrorMessage = "Email can not more than 50 characters.")]
        [Remote("IsUniqueEmail", "UserValidation", AdditionalFields = "x")]
        public string UserEmail { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        [MaxLength(150, ErrorMessage = "Full Name can not more than 150 characters.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(5, ErrorMessage = "Password can not less than 5 characters.")]
        [MaxLength(20, ErrorMessage = "Password can not more than 20 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Password Confirmation is required.")]
        [Compare("Password", ErrorMessage = "Password Confirmation not match.")]
        [DataType(DataType.Password)]
        public string PasswordConfirmation { get; set; }

        [Remote("IsRoleExist", "RoleValidation")]
        public string RoleId { get; set; }

        //[Required(ErrorMessage = "Company is required.")]
        //[Remote("IsExist", "CompanyValidation")]
        //public string CompanyId { get; set; }

        public bool IsActive { get; set; }
        public string[] WarehouseIds { get; set; }
        public string DefaultWarehouseId { get; set; }
    }
}