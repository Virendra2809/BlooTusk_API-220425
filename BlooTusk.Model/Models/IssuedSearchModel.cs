using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class IssuedSearchModel
    {
        public int? MerchantId { get; set; }
        public int? CouponId { get; set; }
        public string? KeyWord { get; set; }
    }
}
