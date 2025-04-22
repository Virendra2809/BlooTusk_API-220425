using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class SmsTemplateModel
    {
        public int TemplateId { get; set; }

        public int? MessageTypeId { get; set; }


        public int? ModeTypeId { get; set; }

        public int? MerchantId { get; set; }

        public string? MessageContent { get; set; }
        public string? MessageType { get; set; }
        

        public string? RecStatus { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int? ModifyBy { get; set; }

        public DateTime? ModifyDate { get; set; }

        public string? organizationName { get; set; }
        public string? contactPersonName { get; set; }
        public string? phoneNumber { get; set; }
        public string? email { get; set; }

        
        
       
    }
}
