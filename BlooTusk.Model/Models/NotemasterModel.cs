using System;
using System.Collections.Generic;

namespace BlooTusk.Model.Models
{
    public partial class NotemasterModel
    {
        public int NotemasterId { get; set; }
        public string Note { get; set; } = null!;
        public string? RecStatus { get; set; }
        public int? CustomerId { get; set; }
        public int? CreatedBy { get; set; }
    }
}
