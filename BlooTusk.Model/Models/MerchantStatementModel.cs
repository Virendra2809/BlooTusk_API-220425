using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class MerchantStatementModel
    {
        public string? MerchantName { get; set; }
        public int? OpeningBalance { get; set; }
        public virtual ICollection<MerchantTransaction> MerchantTransactions { get; set; } = new List<MerchantTransaction>();
    }


    public class MerchantTransaction
    {
   public int? CustomerId { get; set; }
    public int? BalancePoints { get; set; }
    public int? Points { get; set; }
    public string? Transaction { get; set; }
    public int? TransactionType { get; set; }
    public int? CouponsCount { get; set; }
    public int? BalPoint { get;set; }
    public DateTime? TransactionDate { get; set; }
   
    }



 
}
