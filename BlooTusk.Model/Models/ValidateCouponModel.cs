using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class ValidateCouponModel
    {     
        public string? phoneNumber { get; set; } //SubmitedBy  
        public string? CouponCode { get; set; }
        public string? MerchantCode { get; set; }
        public bool? IsNewCustomer { get; set; }
        public int? MerchantId { get; set; }
        public string?  RefferBy { get; set; } // RefferBy

        public int? RefferalPoint { get; set; }

        //public int? RefCustId { get; set;}
        // public int? CustomerId { get; set;}
    }
}
