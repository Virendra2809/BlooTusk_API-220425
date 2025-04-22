using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Merchantsystemuser
{
    public int MerchantSystemUserId { get; set; }

    public int MerchantId { get; set; }

    public string? Name { get; set; }

    public string? PhoneNumber { get; set; }

    public string? UserId { get; set; }

    public string? Password { get; set; }

    public string? RecStatus { get; set; }

    public sbyte? IsAdmin { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyDate { get; set; }

    public string? DeviceId { get; set; }

    public string? DeviceOs { get; set; }

    public string? Token { get; set; }

    public byte[]? Image { get; set; }

    public string? Email { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Merchant Merchant { get; set; } = null!;
}
