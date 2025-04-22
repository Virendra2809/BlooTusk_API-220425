using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class LoginModel
    {
        public string? Username { get; set; }= string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string DeviceID { get; set; } = string.Empty;
        public string DeviceOS { get; set; } = string.Empty;
        public string FirebaseToken { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public sbyte? IsAdmin { get; set; }
    }
}
