using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Rewardpointmaster
{
    public int RewardPonitId { get; set; }

    public int? RewardPoint { get; set; }

    public int? RewardTypeId { get; set; }

    public int? MerchantId { get; set; }

    public int? IssuedBy { get; set; }

    public int? Validity { get; set; }

    public string? RewardDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? IsAdmin { get; set; }

    public int? RecStatus { get; set; }
}
