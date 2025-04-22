using System;
using System.Collections.Generic;

namespace BlooTusk.Entity.BlooTuskModel;

public partial class Countrymaster
{
    public int CountryId { get; set; }

    public string CountryName { get; set; } = null!;

    public virtual ICollection<Po> Pos { get; set; } = new List<Po>();

    public virtual ICollection<Statemaster> Statemasters { get; set; } = new List<Statemaster>();
}
