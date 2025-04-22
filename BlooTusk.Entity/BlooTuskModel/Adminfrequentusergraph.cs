using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Adminfrequentusergraph
{
    public int? Yy { get; set; }

    public int? Mm { get; set; }

    public string? MonthText { get; set; }

    public long TotalFrequentUserCount { get; set; }

    public long TotalVisitCount { get; set; }

    public int MerchantId { get; set; }
}
