using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class APIResponse
    {
        public int ResponseStatusCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ResponseMessage { get; set; } = string.Empty;
        public Object? ResponseData { get;set; }
    }
}
