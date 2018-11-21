using EmailService.Common;
using EmailService.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
            if (processStopCount >= 2)
            {
                Config.log.Warn("------开始 发送软件运行异常报告------");
                Runtime.ShowLog("------开始 发送软件运行异常报告------");
                if (SendProcesssReport(pStates) > 0)
                {
                    Config.log.Warn("------完成 发送软件运行异常报告------");
                    Runtime.ShowLog("------完成 发送软件运行异常报告------");
                }
                else
                {
                    Config.log.Warn("------！！ 发送软件运行异常报告 失败！！------");
                    Runtime.ShowLog("------！！ 发送软件运行异常报告 失败！！------");
                }

            }

            await Console.Out.WriteLineAsync("**********Task.CompletedTask from CheckProcessJob!");
            //await Task.CompletedTask;

        }

        public int SendProcesssReport(List<ProcessState> processStates)
        {
            try
            {
                int CheckTime = Convert.ToInt16(Config.GetValue("CheckTime"));

                //发件人邮箱
                string MailFrom = Config.GetValue("MailFrom");
                string MailSshPwd = Config.GetValue("MailSshPwd");
                //收件人地址，抄送地址（多个地址使用英文分号分割）
                string MailToStr = Config.GetValue("MailToStr");
                string MailToCcStr = Config.GetValue("MailToCcStr");
                //接收软件运行异常告警邮箱地址
                string ReceiverAlarm = Config.GetValue("ReceiverAlarm");

                //邮件主题及内容
                string MailSubject = Config.GetValue("MailSubject");
                string MailBody = Config.GetValue("MailBody");
                //邮件附件所在文件夹路径
                string MailAttachmentsPath = Config.GetValue("MailAttachmentsPath");
                //定时发送时间
                string MailSendTime = Config.GetValue("MailSendTime");

                DateTime nowTime = DateTime.Now;
                string emailBody = "**********  监测到软件运行异常，可能影响系统正常运行; 请及时检查电力监控系统是否运行正常？ ********* \n  当前所监测软件运行状态如下：\n";
                foreach (var ps in processStates)
                {
                    if (ps.State == 1)
                        emailBody += "  软件名称: " + ps.ProcessName + ";  状态 : 运行  \n";
                    else
                        emailBody += "  软件名称: " + ps.ProcessName + ";  状态 : 停止（或异常） \n";
                }

                emailBody += "******************************************************************************************************"
                    + " \n  本次监测时间：" + nowTime.ToString("yyyy-MM-dd HH:mm:ss");

                Email myEmail = new Email();
                myEmail.host = "smtp.163.com";
                myEmail.mailSshPwd = MailSshPwd;
                myEmail.mailFrom = MailFrom;
                myEmail.mailToArray = ReceiverAlarm.Split(';');
                myEmail.mailCcArray = MailToCcStr.Split(';');
                myEmail.mailSubject = "监测软件运行异常提示 " + "(" + nowTime.ToString("yyyy-MM-dd HH:mm") + ") ";
                myEmail.mailBody = emailBody;

                //myEmail.attachmentsPath = attachFileList.ToArray();

                if (myEmail.Send())
                {
                    Config.log.Info("Email: 软件监测告警 发送 成功!");
                    Runtime.ShowLog("Email: 软件监测告警 发送 成功!");

                    // 将发送成功的邮件 存入数据库
                    SaveEmailModel eModel = new SaveEmailModel();
                    eModel.Sender = myEmail.mailFrom;
                    eModel.Receiver = ReceiverAlarm;
                    eModel.CarbonCopy = MailToCcStr;
                    eModel.SendTime = nowTime.ToString("yyyy-MM-dd HH:mm:ss");
                    eModel.SendState = 1;
                    eModel.Subject = myEmail.mailSubject;
                    eModel.Body = myEmail.mailBody;

                    int sqlResult = SetEmailToDB(eModel);
                    if (sqlResult == 1)
                    {
                        Config.log.Info("Email 软件监测告警 写入数据库完成！");
                        Runtime.ShowLog("Email 软件监测告警 写入数据库完成！");
                    }
                    else
                    {
                        Config.log.Info("Email 软件监测告警 写入数据库 失败！");
                        Runtime.ShowLog("Email 软件监测告警 写入数据库 失败！");
                    }

                    return 1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Config.log.Info("Email 软件监测告警 发送 失败! 详细：" + ex.Message);
                Runtime.ShowLog("Email 软件监测告警 发送 失败! 详细：" + ex.Message);
                return 0;
            }
        }

        public static int SetEmailToDB(SaveEmailModel emailModel)
        {
            string sql = @"INSERT INTO SendEmailResult ( Sender,Receiver,CarbonCopy,SendTime,State,MailSubject,MailBody,Attachments)
                                                 VALUES(@Sender,@Receiver,@CarbonCopy,@SendTime,@State,@MailSubject,@MailBody,@Attachments); ";

            SQLiteParameter[] parameters =  {
                                new SQLiteParameter("@Sender", emailModel.Sender),
                                new SQLiteParameter("@Receiver",emailModel.Receiver),
                                new SQLiteParameter("@CarbonCopy",emailModel.CarbonCopy),
                                new SQLiteParameter("@SendTime",emailModel.SendTime),
                                new SQLiteParameter("@State",emailModel.SendState),
                                new SQLiteParameter("@MailSubject",emailModel.Subject),
                                new SQLiteParameter("@MailBody",emailModel.Body),
                                new SQLiteParameter("@Attachments", emailModel.Attachment)
                             };

            int sqlResult = SqliteHelper.ExecuteNonQuery(sql, parameters);
            if (sqlResult >= 1)
            {
                //Config.log.Info("Email 存入数据库 成功!");
                return 1;
            }
            else
            {
                //Config.log.Info("Email 存入数据库 失败!");
                return 0;
            }

        }



    }
}
