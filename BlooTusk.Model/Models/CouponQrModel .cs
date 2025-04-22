using System;
using System.Collections.Generic;

namespace BlooTusk.Model.Models
{
public partial class CouponQrModel
{
    public string? CouponTitle { get; set; }
    public string? CouponDiscerption { get; set; }
    public string CouponCode { get ; set; }
    public DateTime EndDate { get; set; }
    public string? DiscountType { get; set; }
    public int? DiscountValue { get; set; }
    public string? MerchantName { get; set; }

        public string? Address { get; set; }


    }
}
