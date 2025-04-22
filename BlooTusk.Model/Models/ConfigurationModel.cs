using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class ConfigurationModel
    {
        public string HostName { get; set; }
        public string ACCOUNT_SID { get; set; }
        public string AUTH_TOKEN { get; set; }
        public string FromMobileNumber { get; set; }
        public string SiteUrl { get; set; }
        public string MobileCountryCode { get; set; }
        public string SymmetricKey { get; set; }
        public string MerchantSignUpURL { get; set; }
        public bool Bypassgetway { get; set; }

        public int MessageLimit { get; set; }





    }
}
