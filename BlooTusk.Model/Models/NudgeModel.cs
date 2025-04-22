using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class NudgeModel
    {
        public int? CouponId { get; set; }
        public string? DiscountType { get; set; }
        public int? DiscountValue { get; set; }
        public string MerchantName { get; set; }
    }
}
