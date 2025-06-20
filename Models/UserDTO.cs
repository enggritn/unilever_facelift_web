using System;

namespace Facelift_App.Models
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string UserEmail { get; set; }
        public string UserPassword { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        //public string CompanyName { get; set; }
        public string LastVisitUrl { get; set; }
        public string LastVisitAt { get; set; }
        public string LastLoginAt { get; set; }
        public string IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }
        public string ChPassAt { get; set; }
    }
}