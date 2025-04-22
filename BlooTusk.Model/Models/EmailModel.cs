using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Model.Models
{
    public class EmailModel
    {
        public EmailModel() {
            EmailSettings=new EmailSettings();
        }

        public string ReceiverEmail { get; set; }
        public string Subject { get; set; }
        public string EmailBody { get; set; }
      
        public EmailSettings EmailSettings { get; set; }

        
    }
}
