using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class DashboardDataModel
    {
        public int SignUpUsers { get; set; } = 0;
        public int TiedUsers { get; set; } = 0;
        public int RepeatedUser { get; set; } = 0;
        public int ClosedReffralUsers { get; set; } = 0;
    }
}
