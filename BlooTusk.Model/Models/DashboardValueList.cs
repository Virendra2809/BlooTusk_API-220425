using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class DashboardValueList
    {
        public int MerchantId { get; set; }
        public string? Keyword { get; set; }
        public string? Recent { get; set; }
        public string? Name { get; set; }
        public int PageNumber { get; set; }
    }
}
