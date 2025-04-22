using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class CustomerRefferSearchResultModel
    {
        public CustomerRefferSearchResultModel()
        {
            CustomerList = new List<CustomerModel>();
        }

        public string city { get; set; }

        public string RewardPoint { get; set; }

        public double PageCount { get; set; }

        public List<CustomerModel> CustomerList { get; set; }
    }
}
