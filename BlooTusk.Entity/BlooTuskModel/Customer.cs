using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? Name { get; set; }

    public string? PhoneNumber { get; set; }

    public string? CustomerCode { get; set; }

    public sbyte? IsPhoneNumberValidate { get; set; }

    public sbyte? StopMessage { get; set; }

    public string? RecStatus { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyDate { get; set; }

    public string? Lastname { get; set; }

    public virtual ICollection<Customermerchantmapper> Customermerchantmappers { get; set; } = new List<Customermerchantmapper>();
}
