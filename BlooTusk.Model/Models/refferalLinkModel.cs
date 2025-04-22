using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class refferalLinkModel
    {
        public string? CustomerCode { get; set; }

        public string? MerchantCode  { get; set; }

        public string? refferalCode { get; set; }
        public string MerchantName { get; set; }
        public int? CountOfRefferalLink { get; set; }
        public List<refferallist> referlist { get; set; }

    }

    public class refferallist
    {
        public int UserCustId { get; set; }

        public bool? StopSms { get;set; }
        public string? MerchantName { get; set; }
        public string? REfferalLink { get; set; }
    }

}
