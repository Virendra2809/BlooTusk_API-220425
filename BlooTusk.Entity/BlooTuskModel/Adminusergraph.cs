using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Adminusergraph
{
    public int MerchantId { get; set; }

    public string? Yy { get; set; }

    public string? MonthText { get; set; }

    public int? Month { get; set; }

    public long TotalCustomers { get; set; }

    public decimal? ReferralCount { get; set; }

    public decimal? WalkInCount { get; set; }
}
