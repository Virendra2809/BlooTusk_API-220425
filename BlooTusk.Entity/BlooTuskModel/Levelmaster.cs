using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Levelmaster
{
    public int LevelId { get; set; }

    public int RequirePoint { get; set; }

    public string Level { get; set; } = null!;

    public int RecStatus { get; set; }
}
