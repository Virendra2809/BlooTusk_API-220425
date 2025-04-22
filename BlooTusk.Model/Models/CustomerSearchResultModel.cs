using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class CustomerSearchResultModel
    {
         public string? Level { get; set; }
        public int? NoOfVisit { get; set; }
        public string LastVisited { get; set; }
        public int? Trustscore { get; set; }
        public int? BlotuskPoint { get; set; }


        public int? CustomerID { get; set; }
        public string? CustomerCode { get; set; }
        public int MerchantID { get; set; }
        public string? MerchantCode { get; set; }

        public string? Name { get; set; }

        public string? PhoneNumber { get; set; }

        public sbyte IsPhoneNumberValidate { get; set; }

        public sbyte StopMessage { get; set; }

        public string ReferCode { get; set; }

        public int ReferBy { get; set; }

        public int? RewardPoint { get; set; }

        public string ApprovalStatus { get; set; }

        public string RecStatus { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int? ModifyBy { get; set; }

        public string MerchantName { get; set; }

        public DateTime? ModifyDate { get; set; }

        public bool? isSelected { get; set; }

        public bool? isVisibleMenu { get; set; }
       
       public double PageCount { get; set; }

        public List<CustomerModel> CustomerList { get; set; }
    }
}
