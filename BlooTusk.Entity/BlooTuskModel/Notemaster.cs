using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Notemaster
{
    public int NotemasterId { get; set; }

    public string Note { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public string? RecStatus { get; set; }

    public int? CustomerId { get; set; }

    public int? CouponId { get; set; }

    public bool? Specialoffer { get; set; }
}
