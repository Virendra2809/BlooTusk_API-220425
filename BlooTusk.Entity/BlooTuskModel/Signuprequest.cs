using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Signuprequest
{
    public int Id { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? CategoryName { get; set; }

    public string? Ipaddress { get; set; }

    public DateTime? CreatedDate { get; set; }
}
