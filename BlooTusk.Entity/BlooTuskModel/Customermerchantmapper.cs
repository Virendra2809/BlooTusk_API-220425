using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Customermerchantmapper
{
    public int UserMerchantMapperId { get; set; }

    public int MerchantId { get; set; }

    public int CustomerId { get; set; }

    public string? ReferCode { get; set; }

    public int? ReferBy { get; set; }

    public string? ApprovlStatus { get; set; }

    public bool? StopMessage { get; set; }

    public DateTime? Createdate { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Merchant Merchant { get; set; } = null!;
}
