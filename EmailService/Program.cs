using EmailService.CheckProcess;
using EmailService.EnergyDataJob;
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
        private static System.Threading.Mutex mutex;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //防止重复运行软件
            mutex = new System.Threading.Mutex(true, "OnlyRun");
            if (mutex.WaitOne(0, false))
            {
                RunProgramRunExample().GetAwaiter().GetResult();
                Application.Run(new Main());
            }
            else
            {
                MessageBox.Show(" 软件已运行！请勿重复运行此软件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
            }
            //RunProgramRunExample().GetAwaiter().GetResult();

        }

        private static async Task RunProgramRunExample()
        {
            try
            {
                
                // Grab the Scheduler instance from the Factory
                //NameValueCollection props = new NameValueCollection
                //{
                //    { "quartz.serializer.type", "binary" }
                //};
                StdSchedulerFactory factory = new StdSchedulerFactory();
                IScheduler scheduler = await factory.GetScheduler();

                // and start it off
                await scheduler.Start();

                /*
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
                        .WithIntervalInMinutes(2)
                        .RepeatForever())
                    .Build();

                //job3
                IJobDetail sentReportJob = JobBuilder.Create<SentReportJob>()
                   .WithIdentity("sentReportJob", "group2")
                   .Build();

                // Trigger the job to run now, and then repeat every 10 seconds
                ITrigger sentReportTrigger = TriggerBuilder.Create()
                    .WithIdentity("sentReportTrigger", "group2")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(30)
                        .RepeatForever())
                    .Build();

                // Tell quartz to schedule the job using our trigger
                await scheduler.ScheduleJob(job, trigger);
                //await scheduler.ScheduleJob(processJob, processTrigger);
                await scheduler.ScheduleJob(sentReportJob, sentReportTrigger);


                await scheduler.Start();
                */

            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se);
            }
        }

    }
}
