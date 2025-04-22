using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Couponissuedetail
{
    public int CouponIssueDetailsId { get; set; }

    public int CouponIssueMasterId { get; set; }

    public int CouponId { get; set; }

    public string CouponSerialNo { get; set; } = null!;

    public int CustomerId { get; set; }

    public int UsedbyId { get; set; }

    public int? RedeemId { get; set; }

    public int? RecStatus { get; set; }

    public string? ModifyBy { get; set; }

    public string? ModifyDate { get; set; }

    public string? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? IssuedDate { get; set; }

    public bool? Specialoffer { get; set; }
}
