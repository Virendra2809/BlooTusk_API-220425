using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class MerchantAuthenticateModel
    {
        public int MerchantID { get; set; } = 0;
        public int MerchantSystemUserID { get; set; } = 0;
        public int PosId { get; set; } = 0;
        public string UserID { get; set; } = string.Empty;
        public string MerchantName { get; set;} = string.Empty;
        public string Email { get; set;} = string.Empty;
        public string PhoneNumber { get; set;} = string.Empty;
        public string Token { get; set;} = string.Empty;
        public string ApprovalStatus { get; set;} = string.Empty;
        public string RecStatus { get; set;} = string.Empty;
        public sbyte? IsAdmin { get; set; }
        public string? ImageUrl { get; set;} 

        public string? Branch { get; set; }

        public string? City { get; set; }

        public string? MerchantURL { get; set; }
    }
}
