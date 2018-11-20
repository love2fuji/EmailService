using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailService.Test
{
    public class HelloJob : IJob
    {
        NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        public async Task Execute(IJobExecutionContext context)
        {
            await Console.Out.WriteLineAsync("Greetings from HelloJob!");
            log.Info("Greetings from HelloJob!");
        }
    }
}
