using System;
using System.Collections.Generic;

namespace BlooTusk.Model.Models;

public partial class CustomerMerchantModel
{
    public int UserMerchantMapperId { get; set; }

    public bool? StopMessage { get; set; }

    public int MerchantId { get; set; }

    public int CustomerId { get; set; }

    public string? ReferCode { get; set; }

    public int? ReferBy { get; set; }

    public string? ApprovlStatus { get; set; }

}
