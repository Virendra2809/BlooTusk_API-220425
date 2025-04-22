using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Po
{
    public int Posid { get; set; }

    public int MerchantId { get; set; }

    public int CategoryId { get; set; }

    public string? Poscode { get; set; }

    public string? Posname { get; set; }

    public string? Posaddress { get; set; }

    public string? Zip { get; set; }

    public int? StateId { get; set; }

    public int? CountryId { get; set; }

    public string? Latitude { get; set; }

    public string? Longitude { get; set; }

    public string? RecStatus { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyDate { get; set; }

    public virtual Categorymaster Category { get; set; } = null!;

    public virtual Countrymaster? Country { get; set; }

    public virtual Merchant Merchant { get; set; } = null!;

    public virtual Statemaster? State { get; set; }
}
