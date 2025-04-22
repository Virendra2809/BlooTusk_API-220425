using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Messagetypemaster
{
    public int MessageTypeId { get; set; }

    public string? MessageType { get; set; }

    public string? RecStatus { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyDate { get; set; }

    public string? ModeType { get; set; }

    public virtual ICollection<Smstemplate> Smstemplates { get; set; } = new List<Smstemplate>();
}
