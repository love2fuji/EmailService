using EmailService.Common;
using EmailService.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailService.EnergyDataJob
{
    public class SentReportJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                /// <summary>
                /// 待发送全部附件的路径（一个文件夹内的所有Excel文件）
                /// </summary>
                List<string> mailAttachmentsList = new List<string>();
                //邮件附件所在文件夹路径
                string MailAttachmentsPath = Config.GetValue("MailAttachmentsPath");
                //将已经发送的成功的文件移动到目标文件夹
                string TargetPath = Config.GetValue("TargetPath");

                if (Directory.Exists(MailAttachmentsPath))
                {
                    string[] files = Directory.GetFiles(MailAttachmentsPath);

                    Runtime.ShowLog("----- 开始 添加附件：-----");
                    Config.log.Info("----- 开始 添加附件：-----");
                    foreach (string s in files)
                    {
                        // Use static Path methods to extract only the file name from the path.
                        string fileName = Path.GetFileName(s);
                        Config.log.Info(" 附件名称：" + fileName);

                        //判断文件文件是否已发送，未发送文件则添加到待发送列表
                        string sql = @"SELECT COUNT(1) FROM EmailAttachmentsState WHERE AttachmentsName LIKE '%" + fileName + "%';";
                        int sqlResult = Convert.ToInt16(SqliteHelper.ExecuteScalar(sql, null));
                        if (sqlResult == 0)
                        {
                            mailAttachmentsList.Add(s);
                            Config.log.Info("新增的邮件附件： " + s);
                            Runtime.ShowLog("新增的邮件附件： " + s);
                        }
                        else
                        {
                            Config.log.Warn("已发送的邮件附件： " + s);
                        }

                    }
                    Runtime.ShowLog("----- 完成 添加附件：-----");
                    Config.log.Info("----- 完成 添加附件：-----");

                    Config.log.Info("----- 开始 发送邮件 -----");
                    Runtime.ShowLog("----- 开始 发送邮件 -----");
                    if (SendEnergyReport(mailAttachmentsList) == 1)
                    {

                        Config.log.Info("Email: 用能报表 发送 成功!");
                        Runtime.ShowLog("Email: 用能报表 发送 成功!");

                        Config.log.Info("----- 完成 发送邮件 -----");
                        Runtime.ShowLog("----- 完成 发送邮件 -----");

                        // 将发送成功的附件 存入数据库
                        foreach (string item in mailAttachmentsList)
                        {
                            //eModel.Attachment += item;
                            string fileName = Path.GetFileName(item);
                            int sqlResult = SaveEmailToDB.SetAttachmentToDB(fileName, 1, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            if (sqlResult == 1)
                            {
                                Config.log.Info("DB: 用能报表 " + fileName + "写入数据库 成功");
                            }
                            else
                            {
                                Config.log.Info("DB: 用能报表 " + fileName + "写入数据库 失败！");
                            }
                        }

                        Config.log.Info("----- 开始 移动文件 -----");
                        Runtime.ShowLog("----- 开始 移动文件 -----");
                        if (MoveAllFiles(MailAttachmentsPath, TargetPath) > 0)
                        {
                            Config.log.Info("----- 完成 移动文件 -----");
                            Runtime.ShowLog("----- 完成 移动文件 -----");
                        }
                        else
                        {
                            Config.log.Info("----- 移动文件 失败 -----");
                            Runtime.ShowLog("----- 移动文件 失败 -----");
                        }
                    }
                }
                else
                {
                    Config.log.Info("----- 能耗报表路径不存在，请检查路径是否正确！");
                    Runtime.ShowLog("----- 能耗报表路径不存在，请检查路径是否正确！");
                }
                
                Config.log.Info("***** 能耗报表任务执行完成：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                Runtime.ShowLog("***** 能耗报表任务执行完成：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                await Console.Out.WriteLineAsync("***** 能耗报表任务执行完成：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch (Exception ex)
            {
                Config.log.Info("----- 能耗报表操作 失败！ 详细：" + ex.Message);
                Runtime.ShowLog("----- 能耗报表操作 失败！ 详细：" + ex.Message);
            }
        }

        /// <summary>
        /// 发送软件运行状态异常告警提示
        /// </summary>
        /// <param name="SendEnergyReport"></param>
        /// <returns>返回：1:发送成功 ; 0:发送失败 </returns>
        public int SendEnergyReport(List<string> emailAttachmentsList)
        {
            try
            {
                //发件人邮箱
                string MailFrom = Config.GetValue("MailFrom");
                string MailSshPwd = Config.GetValue("MailSshPwd");
                //收件人地址，抄送地址（多个地址使用英文分号分割）
                string MailToStr = Config.GetValue("MailToStr");
                string MailToCcStr = Config.GetValue("MailToCcStr");

                //邮件主题及内容
                string MailSubject = Config.GetValue("MailSubject");
                string MailBody = Config.GetValue("MailBody");

                DateTime nowTime = DateTime.Now;
                //string emailBody = "";

                Email myEmail = new Email();
                myEmail.host = "smtp.163.com";
                myEmail.mailSshPwd = MailSshPwd;
                myEmail.mailFrom = MailFrom;
                myEmail.mailToArray = MailToStr.Split(';');
                myEmail.mailCcArray = MailToCcStr.Split(';');
                myEmail.mailSubject = MailSubject + "(" + nowTime.AddDays(-1).ToString("yyyy-MM-dd") + ") ";
                //判断附件是否为空
                if (emailAttachmentsList.Count == 0 || emailAttachmentsList == null)
                {
                    myEmail.mailBody = "内容：尊敬的用户，新的的用能报表尚未生成 " + "(" + nowTime.AddDays(-1).ToString("yyyy-MM-dd") + ")"
                        + " ，请检查系统是否运行正常";
                }
                else
                {
                    myEmail.mailBody = MailBody + "(" + nowTime.AddDays(-1).ToString("yyyy-MM-dd") + ")";
                    myEmail.attachmentsPath = emailAttachmentsList.ToArray();
                }


                if (myEmail.Send())
                {
                    // 将发送成功的邮件 存入数据库
                    SaveEmailModel eModel = new SaveEmailModel();
                    eModel.Sender = myEmail.mailFrom;
                    eModel.Receiver = MailToStr;
                    eModel.CarbonCopy = MailToCcStr;
                    eModel.SendTime = nowTime.ToString("yyyy-MM-dd HH:mm:ss");
                    eModel.SendState = 1;
                    eModel.Subject = myEmail.mailSubject;
                    eModel.Body = myEmail.mailBody;
                    //添加附件
                    foreach (string item in emailAttachmentsList)
                    {
                        eModel.Attachment += item;

                    }
                    int sqlResult = SaveEmailToDB.SetEmailToDB(eModel);
                    if (sqlResult == 1)
                    {
                        Config.log.Info("Email 用能日报表 写入数据库 完成！");
                    }
                    else
                    {
                        Config.log.Info("Email 用能日报表 写入数据库 失败！");
                    }

                    return 1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Config.log.Info("Email： 用能报表 发送 失败! 详细：" + ex.Message);
                Runtime.ShowLog("Email： 用能报表 发送 失败! 详细：" + ex.Message);
                return 0;
            }
        }


        /// <summary>
        /// 将一个文件夹内全部文件移动到另外一个文件夹中
        /// </summary>
        /// <param name="sourcePath">原文件夹的路径</param>
        /// <param name="targetPath">目标文件夹路径</param>
        /// <returns>返回:1移动成功; 0移动失败</returns>
        public int MoveAllFiles(string sourcePath, string targetPath)
        {
            string fileName = "test.txt";
            string sourceFile = Path.Combine(sourcePath, fileName);
            string destFile = Path.Combine(targetPath, fileName);

            // To copy a folder's contents to a new location:
            // Create a new target folder, if necessary.
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            try
            {
                if (Directory.Exists(sourcePath))
                {
                    //获取一个文件夹内全部文件
                    string[] files = Directory.GetFiles(sourcePath);

                    foreach (string s in files)
                    {
                        // Use static Path methods to extract only the file name from the path.
                        fileName = Path.GetFileName(s);

                        destFile = Path.Combine(targetPath, fileName);

                        if (File.Exists(s))
                        {  // Copy the files and overwrite destination files if they already exist.
                            File.Copy(s, destFile, true);
                            File.Delete(s);
                        }
                    }
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Config.log.Info("----- 移动文件 失败! 详细：" + ex.Message);
                Runtime.ShowLog("----- 移动文件 失败! 详细：" + ex.Message);
                return 0;
            }

        }
    }
}
