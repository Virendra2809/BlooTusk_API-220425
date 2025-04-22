using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class User
{
    public int UserId { get; set; }

    public int? Posid { get; set; }

    public string? Name { get; set; }

    public string? PhoneNumber { get; set; }

    public sbyte? IsPhoneNumberValidate { get; set; }

    public sbyte? StopMessage { get; set; }

    public string? ReferCode { get; set; }

    public string? ReferBy { get; set; }

    public int? RewardPoint { get; set; }

    public string? ApprovalStatus { get; set; }

    public string? RecStatus { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyDate { get; set; }

    public string? UserCode { get; set; }
}
