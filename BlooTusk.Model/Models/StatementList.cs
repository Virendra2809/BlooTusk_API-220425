using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class StatementList
    {
        public int? BalancePoints { get; set; }

        public int? Points { get; set; }
        
        public string? Transaction { get; set; }
        
        public DateTime TransactionDate { get; set; }
    }
}
