using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;
 
public partial class CouponredeemtionResultModel
{
    public string? RefferName { get; set; }

    public string? CouponTitle { get; set; }

    public string? RewardCouponTitle { get; set; }

    public string? CouponDiscerption { get; set; }

    public string? DiscountType { get; set; }

    public int? DiscountValue { get; set; }

    public int? CouponIssueDetailId { get; set; }
    public string? CustomerPhoneNumber { get;set; }

    public string? CustomerName { get; set; }

    public string MerchantName { get; set; }

    public int? RewardPoint { get; set; }

}
