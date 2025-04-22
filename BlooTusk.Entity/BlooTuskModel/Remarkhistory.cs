using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Remarkhistory
{
    public int RemarkHistoryId { get; set; }

    public int MerchantId { get; set; }

    public string? Remark { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyDate { get; set; }

    public DateTime? RemarkDate { get; set; }

    public string? ApprovalStatus { get; set; }

    public virtual Merchant Merchant { get; set; } = null!;
}
