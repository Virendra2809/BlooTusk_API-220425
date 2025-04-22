using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class RewardPointModel
    {

        public int RewardPonitId { get; set; }

        public int? RewardPoint { get; set; }

        public string? RewardType { get; set; }
        public int? RewardTypeID { get; set; }

        public int? MerchantId { get; set; }

        public int? IssuedBy { get; set; }

        public string IssuedByName { get; set; }

        public int? Validity { get; set; }

        public string? RewardDate { get; set; }

        public int? CreatedBy { get; set; }

        public string CreatedDate { get; set; }

        public int? ModifyBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? IsAdmin { get; set; }

        public string? organizationName { get; set; }
        public string? contactPersonName { get; set; }
        public string? phoneNumber { get; set; }
        public string? email { get; set; }

    }

 }
