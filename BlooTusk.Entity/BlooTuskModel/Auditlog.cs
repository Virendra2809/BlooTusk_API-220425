using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Auditlog
{
    public int AuditLogId { get; set; }

    public string? RequestedApiname { get; set; }

    public string? InputData { get; set; }

    public string? RequestStatus { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? CreatedDate { get; set; }
}
