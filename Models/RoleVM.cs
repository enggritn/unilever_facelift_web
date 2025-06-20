using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Models
{
    public class RoleVM
    {
        [Required(ErrorMessage = "Role Name is required.")]
        [MinLength(5, ErrorMessage = "Role Name can not less than 5 characters.")]
        [MaxLength(50, ErrorMessage = "Role Name can not more than 50 characters.")]
        [Remote("IsUniqueName", "RoleValidation", AdditionalFields = "x")]
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<MsMainMenu> MenuList { get; set; }
        public int[] MenuIds { get; set; }

    }
}