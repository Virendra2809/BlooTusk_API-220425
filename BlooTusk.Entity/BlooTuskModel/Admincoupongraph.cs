using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Admincoupongraph
{
    public int? Yy { get; set; }

    public int? Month { get; set; }

    public string? MonthText { get; set; }

    public int? MerchantId { get; set; }

    public decimal? TotalCouponCount { get; set; }

    public decimal? RedeemCount { get; set; }
}
