using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Adminmaster
{
    public int AdminId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string RecStatus { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyDate { get; set; }
}
