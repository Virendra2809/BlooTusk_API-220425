using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Trustscore
{
    public int TrustScoreId { get; set; }

    public int? CustomerId { get; set; }

    public int? TrustScore1 { get; set; }
}
