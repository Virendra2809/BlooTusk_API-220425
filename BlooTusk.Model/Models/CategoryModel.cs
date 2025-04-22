using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace BlooTusk.Model.Models
{
    public class CategoryModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int? RewardPoint { get; set; }
        public string RecStatus { get; set; }
    }
}
