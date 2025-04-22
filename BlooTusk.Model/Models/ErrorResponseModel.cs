using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class ErrorResponseModel
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
    }
}
