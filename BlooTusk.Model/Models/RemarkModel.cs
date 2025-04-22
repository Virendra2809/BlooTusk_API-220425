using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class RemarkModel
    {
        public int RemarkID { get; set; }
        public int MerchantID { get; set; }
        public string ApprovalStatus { get; set; }
        public string Remark { get; set; }
        public DateTime RemarkDate { get; set; }
    }
}
