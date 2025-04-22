using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class AuditlogModel
    {
        public int AuditLogId { get; set; }

        public string? RequestedApiname { get; set; }

        public string? InputData { get; set; }

        public string? RequestStatus { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
