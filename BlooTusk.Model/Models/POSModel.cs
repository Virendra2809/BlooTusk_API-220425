using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class POSModel
    {
        public int Posid { get; set; }

        public int MerchantId { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public string? Poscode { get; set; }

        public string? Posname { get; set; }

        public string? Posaddress { get; set; }

        public string? Zip { get; set; }

        public int? StateId { get; set; }
        public string StateName { get; set; }

        public int? CountryId { get; set; }
        public string CountryName { get; set; }

        public string? Latitude { get; set; }

        public string? Longitude { get; set; }

    }
}
