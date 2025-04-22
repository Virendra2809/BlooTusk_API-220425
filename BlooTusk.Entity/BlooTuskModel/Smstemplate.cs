using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Smstemplate
{
    public int TemplateId { get; set; }

    public int? MessageTypeId { get; set; }

    public int? MerchantId { get; set; }

    public string? MessageContent { get; set; }

    public string? RecStatus { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyDate { get; set; }

    public int? ModeTypeId { get; set; }

    public virtual Merchant? Merchant { get; set; }

    public virtual Messagetypemaster? MessageType { get; set; }
}
