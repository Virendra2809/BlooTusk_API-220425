using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;
 
public partial class CouponredeemtionModel
{
    public int CouponredeemtionId { get; set; }

    public int PosId { get; set; }

    public int MerchantId { get; set; }
    public bool? IsNewCustomer { get; set; }

    public string MerchantCode { get; set; }
    public int CouponIssuedetailsId { get; set; }

    public int RedeembyCustomerId { get; set; }

    public string phoneNumber { get; set; }

    //public int? RecStatus { get; set; }

    //public string? ModifyBy { get; set; }

    //public string? ModifyDate { get; set; }

    //public string? CreatedDate { get; set; }
    public string CouponCode { get; set; }
    public string? CreatedBy { get; set; }
    public int? RedeemRewardPoints { get; set; }
}
