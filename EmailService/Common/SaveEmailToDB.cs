using EmailService.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailService.Common
{
    public class SaveEmailToDB
    {
        /// <summary>
        /// 将邮件内容保存至数据库
        /// </summary>
        /// <param name="emailModel"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailModel"></param>
        /// <returns></returns>
        public static int SetAttachmentToDB(string fileName, int state,string time)
        {
            string sql = @"INSERT INTO EmailAttachmentsState (State,AttachmentsName,Time)
                                                 VALUES(@State,@AttachmentsName,@Time);";

            SQLiteParameter[] parameters =  {
                                new SQLiteParameter("@AttachmentsName", fileName),
                                new SQLiteParameter("@State",state),
                                new SQLiteParameter("@Time",time)
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
