using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class CouponSearchModel
    {
        public int TotalCampagign { get; set; }
        public int LiveCampagign { get; set; }
        public int ExpiredCampagign { get; set; }
        public List<CampagignListModel> campagignList {  get; set; } 

    }



    public class CampagignListModel
    {
        public int CampagingID { get; set; }
        public string CouponTitle { get; set; }
        public string? CouponDiscerption { get; set; }
        public DateTime StartDate { get; set; }
        public string? RecStatus {get;set;}
        public string State { get; set; }
        public string? SelectedUser { get; set; }

        public int? SendCount { get; set; }

        public int? ExpiredCount { get; set; } 

        public int? CouponType { get; set; }
        public DateTime? EndDate { get; set; }

       
        public DateTime? CreatedDate { get; set; }
        public string? DiscountType { get; set; }
        public int? DiscountValue { get; set; }
        public int? Status { get; set; }
        public string MerchantName { get; set; }
        public bool isSelected { get; set; }
        public bool VisibleMenu { get; set; }
        public int? RedeemCount { get; set; }
        public int? NoOfCoupon { get; set; }
        public bool? NudgedCoupon { get; set; }

    }
}
