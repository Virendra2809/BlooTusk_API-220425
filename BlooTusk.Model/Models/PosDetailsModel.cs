using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class PosDetailsModel
    {
        public int MerchantId { get; set; }

        public string CategoryName { get; set; }

        public string? Poscode { get; set; }

        public string? Posname { get; set; }

        public string? Posaddress { get; set; }

        public string? Zip { get; set; }

        public string StateName { get; set; }

        public string CountryName { get; set; }

        public int modifyBy { get; set; }

        public DateTime ModifyDate { get; set; }
    }
}
