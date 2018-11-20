using EmailService.Common;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailService.CheckProcess
{
    public class CheckProcessJob : IJob
    {

        public async Task Execute(IJobExecutionContext context)
        {
            //Config.log.Info("Greetings from CheckProcessJob!");
            ProcessState processState = new ProcessState();
            List<ProcessState> pStates = processState.GetProcessState();
            int processStopCount = 0;
            foreach (var item in pStates)
            {

                Config.log.Info("进程名称：" + item.ProcessName + "  状态：" + item.State + "  时间：" + item.UpdateTime);
                if (item.State == 0)
                {
                    processStopCount++;
                }
            }
            if (processStopCount>=2)
            {
                //SendAlarmEmail();
                Config.log.Warn("发送软件运行异常报告完成");
            }
           
             await Console.Out.WriteLineAsync("**********Task.CompletedTask from CheckProcessJob!");
            //await Task.CompletedTask;

        }
    }
}
