using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class MerchantStatementRequest
    {
        public int? MerchantId { get; set; }

        public string? CustPhoneNo { get; set; }

        public string? FromDate { get; set; }

        public string? ToDate { get; set; }

     
    }
}
