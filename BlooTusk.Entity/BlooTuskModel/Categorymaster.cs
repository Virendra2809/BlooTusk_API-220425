using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Categorymaster
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public int RewardPoint { get; set; }

    public string RecStatus { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyDate { get; set; }

    public virtual ICollection<Po> Pos { get; set; } = new List<Po>();
}
