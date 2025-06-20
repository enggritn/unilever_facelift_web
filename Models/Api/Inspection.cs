using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class Inspection
    {
        [Required(ErrorMessage = "Transaction Id is required.")]
        public string TransactionId { get; set; }

        [Required(ErrorMessage = "Tag Id is required.")]
        public string TagId { get; set; }

        [Required(ErrorMessage = "Reason Name is required.")]
        public string ReasonName { get; set; }


        //[Required(ErrorMessage = "Items is required.")]
        //[MustHaveOneElementAttribute(ErrorMessage = "Item can not be null.")]
        //public List<string> TagId { get; set; }


        //public class MustHaveOneElementAttribute : ValidationAttribute
        //{
        //    public override bool IsValid(object value)
        //    {
        //        var list = value as IList;
        //        if (list != null)
        //        {
        //            return list.Count > 0;
        //        }
        //        return false;
        //    }
        //}

    }
}