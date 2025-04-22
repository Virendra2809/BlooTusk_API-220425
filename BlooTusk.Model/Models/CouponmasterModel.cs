using System;
using System.Collections.Generic;

namespace BlooTusk.Model.Models
{
public partial class CouponmasterModel
{
    public int CouponId { get; set; }

    //public string? CouponCode { get; set; }

    public string? CouponTitle { get; set; }
    public string? CouponDiscerption { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? CouponType { get; set; }
    public string? DiscountType { get; set; }
    public int? DiscountValue { get; set; }
    public int? NoOfCoupon { get; set; }
    public int? MerchantId { get; set; }

  //  public int? CouponTemplateId { get; set; }

   // public int? Status { get; set; }
    public string? RecStatus { get; set; }
   // public int? CreatedBy { get; set; }

  //  public DateTime? CreatedDate { get; set; }

  //  public int? ModifyBy { get; set; }

   // public string? ModifyDate { get; set; }    
  //  public string? MerchantDetail { get; set;}
    public string? SelectedUser { get; set; }
   
    }
}
