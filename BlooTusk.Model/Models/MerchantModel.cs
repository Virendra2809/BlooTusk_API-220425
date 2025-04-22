using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class MerchantModel
    {

        public MerchantModel() {
            this.POSInfo = new POSModel();
        }
        public int MerchantId { get; set; }

        public string? MerchantCode { get; set; }

        public string PhoneNumber { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Password { get; set; }

        public string? City { get; set; }

        public string? OrganizationName { get; set; }

        public string? ContactPersonName { get; set; }

        public string? DeviceId { get; set; }

        public string? DeviceOs { get; set; }

        public string? Token { get; set; }

        public sbyte? IsPhoneNumberValidate { get; set; }

        public sbyte? IsEmailValidate { get; set; }

        public string? ApprovalStatus { get; set; }

        public string? RecStatus { get; set; }

        public string remark { get; set; }

        public string? GeneratedBy { get; set; }

        public string MerchantURL { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifyBy { get; set; }
        public DateTime ModifyDate { get; set; }

        public POSModel POSInfo { get; set; }       
        public double PageCount { get; set; }

        public List<SmsTemplateModel>?  smstemplateList { get; set; }

        public List<RewardPointModel>? RewardPointlist { get; set; }



    }
}
