using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class CustomerRewardSearchModel
    {
        public int? MerchantId { get; set; }
        public int? CustomerId { get; set; }
        public DateTime TransactionDate { get; set; }

        public string TransactionType { get; set; }

        public int RewardPoint { get; set; }



        public int BalancePoint { get; set; }

        public int PageNo { get; set; }


        public double PageCount { get; set; }
        public int PageNumber { get; set; }
        public string Keyword { get; set; }
    }
}
