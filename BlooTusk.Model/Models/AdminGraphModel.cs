using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class AdminGraphModel
    {
        public virtual ICollection<AdminUserGraphModel> AdminUserGraphs { get; set; } = new List<AdminUserGraphModel>();
        public virtual ICollection<AdminFrequentGraphModel> AdminFrequentGraphs { get; set; } = new List<AdminFrequentGraphModel>();
        public virtual ICollection<AdminPointGraphModel> AdminPointGraphs { get; set; } = new List<AdminPointGraphModel>();
        public virtual ICollection<AdminCouponsGraphModel> AdminCouponsGraphs { get; set; } = new List<AdminCouponsGraphModel>();

    
     public string coupondata { get; set; }
        public string monthdata { get; set; }
        public string couponredeemdata { get; set; }

        public string userrefferaldata { get; set; }
        public string userwainindata { get; set; }

        public string usermonthdata { get; set; }

        public string pointearndata { get; set; }
        public string pointreddemdata { get; set; }

        public string pointmonthdata { get; set; }

        public string frequentvistdata { get; set; }
        public string frequentuserdata { get; set; }

        public string frequentmonthdata { get; set; }

    }

    public class AdminUserGraphModel
    {
        public string? MonthText { get; set; }
        public int? Month { get; set; }
        public int MerchantId { get; set; }
        public int? WalkInUser { get; set; }
        public int RefferalUser { get; set; }

    }
    public class AdminFrequentGraphModel
    {
        public string? MonthText { get; set; }
        public int? Month { get; set; }
        public int? MerchantId { get; set; }
        public int TotalFrequentCount { get; set; }
        public int TotalvisitCount { get; set; }
    }
    public class AdminPointGraphModel
    {

        public string? MonthText { get; set; }
        public int? Month { get; set; }
        public int? MerchantId { get; set; }
        public int PointEarned { get; set; }
        public int Pointredeem { get; set; }

    }
    public class AdminCouponsGraphModel
    {
        public string? MonthText { get; set; }
        public int? Month { get; set; }
        public int? MerchantId { get; set; }
        public int Coupons { get; set; }
        public int Couponsredeem { get; set; }
    }
}
