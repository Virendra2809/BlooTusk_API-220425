using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class CustomerInfoModel
    {
        public int CustomerID { get; set; } = 0;
        public string Name { get; set;}=string.Empty;
        public string CustomerCode { get; set;}=string.Empty;
        public string PhoneNumber { get; set; }
        public int MerchantID { get; set; } = 0;
        public string OrganizationName { get; set; } = string.Empty;
        public string MerchantCode { get; set; } = string.Empty;



    }
}
