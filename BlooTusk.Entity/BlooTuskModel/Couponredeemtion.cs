using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Couponredeemtion
{
    public int CouponredeemtionId { get; set; }
    public DateTime CouponredeemtionDate { get; set; }
    public int PosId { get; set; }
    public int CouponIssuedetailsId { get; set; }
    public int RedeembyCustomerId { get; set; }
    public int? RecStatus { get; set; }
    public string? ModifyBy { get; set; }
    public string? ModifyDate { get; set; }
    public string? CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
}
