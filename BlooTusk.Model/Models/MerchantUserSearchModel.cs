using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class MerchantUserSearchModel
    {
        //public string name { get; set; }
        public int MerchantID { get; set; }
        public int PageNumber { get; set; }
        public string Keyword { get; set; }
        public string Role { get;set; }

        public string Recent { get; set; }

    }
}
