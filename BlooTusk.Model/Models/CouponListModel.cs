using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class CouponListModel
    {
        public int CouponId { get; set; }

        public string? CouponCode { get; set; }

        public string? CouponTitle { get; set; }

        public string? CouponDiscerption { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int? CouponType { get; set; }

        public string? DiscountType { get; set; }

        public int? DiscountValue { get; set; }

        public int? NoOfCoupon { get; set; }

        public int? MerchantId { get; set; }

        public int? CouponTemplateId { get; set; }

        public int? Status { get; set; }
       
        public int? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int? ModifyBy { get; set; }

        public DateTime? ModifyDate { get; set; }

        public string? MerchantDetail { get; set; }




    }
}
