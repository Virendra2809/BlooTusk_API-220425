using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class MerchantUserSearchResultModel 
    {
        public MerchantUserSearchResultModel()
        {
            MerchantUserList = new List<MerchantUserModel>();
        }
        public double PageCount { get; set; }
        public int? TotalCount { get; set; }
        public int? UserCount { get; set; }

        public List<MerchantUserModel> MerchantUserList { get; set; }

       
    }
}
