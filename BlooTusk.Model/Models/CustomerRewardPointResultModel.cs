using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class CustomerRewardPointResultModel
    {
      


        public CustomerRewardPointResultModel()
        {
            CustomerList = new List<RewardpointtransactionModel>();
        }
        public double PageCount { get; set; }

        public List<RewardpointtransactionModel> CustomerList { get; set; }
    }
}
