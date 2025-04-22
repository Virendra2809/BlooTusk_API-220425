using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Merchant
{
    public int MerchantId { get; set; }

    public string? MerchantCode { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public string? OrganizationName { get; set; }

    public string? ContactPersonName { get; set; }

    public string? DeviceId { get; set; }

    public string? DeviceOs { get; set; }

    public string? Token { get; set; }

    public sbyte? IsPhoneNumberValidate { get; set; }

    public sbyte? IsEmailValidate { get; set; }

    public string? ApprovalStatus { get; set; }

    public string? RecStatus { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyDate { get; set; }

    public string? City { get; set; }

    public virtual ICollection<Customermerchantmapper> Customermerchantmappers { get; set; } = new List<Customermerchantmapper>();

    public virtual ICollection<Merchantsystemuser> Merchantsystemusers { get; set; } = new List<Merchantsystemuser>();

    public virtual ICollection<Po> Pos { get; set; } = new List<Po>();

    public virtual ICollection<Remarkhistory> Remarkhistories { get; set; } = new List<Remarkhistory>();

    public virtual ICollection<Smstemplate> Smstemplates { get; set; } = new List<Smstemplate>();
}
