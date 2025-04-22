using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class MerchantWiseCustomerCount
    {     
        public int? CustomerVisit { get; set; }       
        public int? FrequentCustomer { get; set; }       
        public int? NewCustomer {  get; set; }
        public int? WalkInCustomer { get; set; }
        public int? RefferalCustomer { get; set; }

        public double percentageCustomerVisits { get; set; }
        public double percentageFrequentCustomers { get; set; }
        public double percentageNewCustomers { get; set; }

        public List<CustomerSearchResultModel> CustomerList { get; set; }




    }
}
