using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class SignupRequestModel
    {
        public int Id { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Ipaddress { get; set; }
        public string? Category { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
