using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Statemaster
{
    public int StateId { get; set; }

    public int? CountryId { get; set; }

    public string? StateName { get; set; }

    public virtual Countrymaster? Country { get; set; }

    public virtual ICollection<Po> Pos { get; set; } = new List<Po>();
}
