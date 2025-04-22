using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class CustomerCouponList
    {
        public string? phoneNumber { get; set; }
      public int? CouponFilter { get; set; }
        public int? MerchantId { get; set; }


        public int? CustomerId { get; set; }
        public int PageNumber { get; set; }    
    }
}
