using System;
using System.Collections.Generic;

namespace BlooTusk.Model.Models
{

public partial class RewardpointtransactionModel
{
        public int RewardTransactionId { get; set; }
        public int? MerchantId { get; set; }
        public int? CustomerId { get; set; }
        public DateTime? TransactionDate { get; set; }   
        public string? TransactionType { get; set; }

        public string? MerchantName { get; set; }
        public int? RewardPoint { get; set; }

        public int BalancePoint { get; set; }
        public int? RewardPointId { get; set; }
}

}

