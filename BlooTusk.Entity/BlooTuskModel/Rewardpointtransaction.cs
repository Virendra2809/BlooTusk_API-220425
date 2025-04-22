using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Rewardpointtransaction
{
    public int RewardTransactionId { get; set; }

    public int? MerchantId { get; set; }

    public int? CustomerId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? Createddate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? Modifieddate { get; set; }

    public DateTime? TransactionDate { get; set; }

    public int? RewardPointId { get; set; }

    public int? TransactionType { get; set; }

    public int? ReffrealId { get; set; }

    public int? Points { get; set; }
}
