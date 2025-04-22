using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model
{
    public class ResetPasswordModel
    {
        public string Password { get; set; }=string.Empty;
        public string Username { get; set; } = string.Empty;
        public int MerchantId { get; set; } = 0;
    }
}
