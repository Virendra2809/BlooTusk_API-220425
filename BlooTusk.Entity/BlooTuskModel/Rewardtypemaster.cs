using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Rewardtypemaster
{
    public int RewardTypeId { get; set; }

    public string? RewardType { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? RecStatus { get; set; }

    public string? ShortName { get; set; }
}
