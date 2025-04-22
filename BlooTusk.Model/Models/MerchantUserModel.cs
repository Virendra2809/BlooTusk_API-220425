using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class MerchantUserModel
    {
        public int MerchantSystemUserId { get; set; }

        public int MerchantId { get; set; }

        public string? Name { get; set; }

        public string? PhoneNumber { get; set; }

        public string? UserId { get; set; }

        public string? Password { get; set; }

        public string? RecStatus { get; set; }
        public bool? isSelected { get; set; }
        public bool? VisibleMenu { get; set; }
        public ulong? IsAdmin { get; set; }

        public byte[]? UserImage { get; set; }
        public string? ImageUrl { get; set; }
        public string? Email { get; set; }


    }
}
