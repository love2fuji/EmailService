using EmailService.CheckProcess;
using EmailService.Test;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmailService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            RunProgramRunExample().GetAwaiter().GetResult();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
            //RunProgramRunExample().GetAwaiter().GetResult();

        }

        private static async Task RunProgramRunExample()
        {
            try
            {
                // Grab the Scheduler instance from the Factory
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                IScheduler scheduler = await factory.GetScheduler();

                // and start it off
                //await scheduler.Start();

                // define the job and tie it to our HelloJob class
                IJobDetail job = JobBuilder.Create<HelloJob>()
                    .WithIdentity("job1", "group1")
                    .Build();

                // Trigger the job to run now, and then repeat every 10 seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(10)
                        .RepeatForever())
                    .Build();

                //3、创建一个触发器另一种方式

                //ITrigger trigger = TriggerBuilder.Create()
                //    .WithIdentity("trigger1", "group1")
                //    .WithCronSchedule("0/5 * * * * ?")     //5秒执行一次
                //    .Build();



                //job2
                IJobDetail processJob = JobBuilder.Create<CheckProcessJob>()
                   .WithIdentity("processJob", "group2")
                   .Build();

                // Trigger the job to run now, and then repeat every 10 seconds
                ITrigger processTrigger = TriggerBuilder.Create()
                    .WithIdentity("processTrigger", "group2")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(30)
                        .RepeatForever())
                    .Build();

                // Tell quartz to schedule the job using our trigger
                await scheduler.ScheduleJob(job, trigger);
                await scheduler.ScheduleJob(processJob, processTrigger);


                await scheduler.Start();
                
            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se);
            }
        }

    }
}
