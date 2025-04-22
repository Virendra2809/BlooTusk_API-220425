using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class CustomerCouponListModel
    {
        public CustomerCouponListModel()
        {
            couponList = new List<CouponList>();
        }
        public double PageCount { get; set; }

        public List<CouponList> couponList { get; set; }
     
    }

    public class CouponList
    {
        public int CouponId { get; set; }

        public string? CouponCode { get; set; }

        public string? CouponTitle { get; set; }

        public string? CouponDiscerption { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? CouponType { get; set; }

        public string? DiscountType { get; set; }

        public string? Source { get; set; }

        public string? Address { get; set; }

        public int? DiscountValue { get; set; }


        public int? Status { get; set; }
        public string? PhoneNumber { get; set; }
        public string? MerchantDetail { get; set; }

        public string? ImageUrl { get; set; }

        public int? TotalRecord { get; set; }

    }
}
