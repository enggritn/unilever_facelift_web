using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class Accident
    {

        [Required(ErrorMessage = "Transaction Id is required.")]
        public string TransactionId { get; set; }

        [Required(ErrorMessage = "Items is required.")]
        [MustHaveOneElementAttribute(ErrorMessage = "Item can not be null.")]
        public List<string> items { get; set; }


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