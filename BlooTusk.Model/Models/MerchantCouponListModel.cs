using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class MerchantCouponListModel
    {
        public int? BlooTuskRewardCount { get; set; }
        public int? BlooTuskRedeemedCount { get; set; }
        public int? CouponsredeemCount { get; set; }
        public int? CouponsSendCount { get; set; }

        public List<CouponListModel> MerchantCouponList { get; set; }
    }
}
