using System;
using System.Collections.Generic;

namespace BlooTusk.Model.Models
{
public partial class CouponSpacialModel
    {
    public int CouponId { get; set; }

        public int MerchantId { get; set; }
    public int CustomerId { get; set; }
        public int? NoOfCoupon { get; set; }

        public string? SelectedUser { get; set; }



    }
}
