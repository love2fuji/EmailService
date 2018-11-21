using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailService.Models
{
    public class SaveEmailModel
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string CarbonCopy { get; set; }
        public string SendTime { get; set; }
        //OK=1; NG=0
        public int SendState { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Attachment { get; set; }
    }
}
