using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Nudgecoupon
{
    public int NudgeCouponId { get; set; }

    public int CouponId { get; set; }

    public DateTime? CreatedDate { get; set; }
}
