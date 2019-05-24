using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailService.Models
{
    public class MeterValueModel
    {
        public string MeterNO { get; set; }
        public string MeterName { get; set; }
        public string TimeValue { get; set; }
        public string UpdateTimeDate { get; set; }
        public string ReadValue { get; set; }
        public string UpdateReadDate { get; set; }
    }
}
