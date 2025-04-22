using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Otplog
{
    public int OtplogId { get; set; }

    public string? Otptype { get; set; }

    public string? SendTo { get; set; }

    public string? Otp { get; set; }

    public string? Otpstatus { get; set; }

    public DateTime? CreatedDate { get; set; }
}
