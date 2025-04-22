using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class AdminDashboardModel
    {          
        public int? Users { get; set; }
        public int? UsersMtd { get; set; }
        public decimal? UserRefferal { get; set; }
        public decimal? UserWalkIn { get; set; }
        public long FrequentUserVisit { get; set; }
        public long FrequentUser { get; set; }

        public int? Merchants { get; set; }
        public int? MerchantsMtd { get; set; }

        public int? MerchantsPending { get; set; }

        public int? Points { get; set; }
        public int? PointsMtd { get; set; }
        public int? SignInPoint { get; set; }
        public int? RefferalPoint { get; set; }
       // public int? RedeemePoints { get; set; }
        public decimal EarnPoint { get; set; }
        public decimal ReddemPoints { get; set; }
        public int? ReddemPointsMtd { get; set; }


        //

        public int? Campaign { get; set; }
        public int? CampaignMtd { get; set; }
        public decimal? Coupon { get; set; }
        public decimal? CouponRedeem { get; set; }
        public int? CouponTransfer { get; set; }






    }
}
