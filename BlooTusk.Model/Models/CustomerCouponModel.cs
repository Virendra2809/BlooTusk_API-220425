using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class CustomerCouponModel
    {
        public int CouponId { get; set; }

        public string? CouponCode { get; set; }

        public string? CouponTitle { get; set; }

        public string? CouponDiscerption { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int? CouponType { get; set; }

        public string? DiscountType { get; set; }

        public string? RedeemName { get; set; }

        public DateTime RedeemDate { get; set; }

        public int? DiscountValue { get; set; }

        public int? Status { get; set; }
        public string? PhoneNumber { get; set; }
        public string? MerchantDetail { get; set; }

    }
}
