using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class MerchantSearchResultModel
    {
        public MerchantSearchResultModel()
        {
            MerchantList = new List<MerchantModel>();
        }
        public double PageCount { get; set; }

        public List<MerchantModel> MerchantList { get; set; }
    }
}
