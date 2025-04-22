using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace BlooTusk.Model.Models
{
public partial class CampainListSerach
    {
    public string? Recent { get; set; }
    public string? Type { get; set; }
    public string? Performance { get; set; }
    public int? MerchantId { get; set; }

        public int? CustomerId { get; set; }
        public int PageNumber { get; set; }



    }
}
