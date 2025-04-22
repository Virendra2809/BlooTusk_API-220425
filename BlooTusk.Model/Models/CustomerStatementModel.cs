using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models 
{
    public class CustomerStatementModel
    {

        public string? CustomerName { get; set; }
        public int? OpeningBalance { get; set; }
        public virtual ICollection<CustomerTransaction> CustomerTransactions { get; set; } = new List<CustomerTransaction>();
           
    }

    public class CustomerTransaction
    {
        public int? CustomerId { get; set; }
        public int? BalancePoints { get; set; }

        public int? BalPoint { get; set; }
        public int? CouponsCount { get; set; }
        public int? Points { get; set; }
        public string? Transaction { get; set; }
        public DateTime? TransactionDate { get; set; }
    }
}
