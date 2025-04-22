using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class ValidateCouponSearchModel
    {     
        public string? phoneNumber { get; set; }
        public string? CouponCode { get; set; }
        public string? RecLink { get; set; }
        public int? MerchantId { get; set; }
        public string DiscountType { get; set; }
        public int? DiscountValue { get; set; }
        public string? CouponTitle { get; set; }
        public string? CouponDiscerption { get; set; }
        public string CustomerName { get; set; }
        public string? CustomerRefferalName { get; set; }
        public string? MerchantName { get; set;}
        public int? CustomerId { get;set; }
        public int? RefCustomerId {  get; set; } 


    }
}
