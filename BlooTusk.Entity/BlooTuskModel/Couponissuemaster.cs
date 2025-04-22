using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Couponissuemaster
{
    public int CouponIssuemasterId { get; set; }

    public int CouponId { get; set; }

    public DateTime IssueDate { get; set; }

    public int IssuedbyId { get; set; }

    public int IssuedQty { get; set; }

    public string IssuedTo { get; set; } = null!;

    public int? RecStatus { get; set; }

    public string? ModifyBy { get; set; }

    public string? ModifyDate { get; set; }

    public string? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }
}
