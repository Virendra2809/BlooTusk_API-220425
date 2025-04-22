using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class CustomerSearchModel
    {
        public int MerchantID { get; set; }
        public int CustomerID { get; set; }
        public int PageNumber { get; set; }
        public string Keyword { get; set; }
        public string PhoneNumber { get; set; }
    }
}
