using Dapper;
using EmailService.Common;
using EmailService.EnergyDataJob;
using EmailService.WorkJob;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmailService
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();

        }

        private void Main_Load(object sender, EventArgs e)
        {
            Runtime.ServerLog = this.ServerLog;
            Runtime.ShowLog("------ 启动软件 ------");
            Runtime.m_IsRunning = false;


            Runtime.ShowLog("初始化软件");
            Config.log.Info("初始化软件");
            txtBoxEmailAddress.Text = Config.GetValue("MailToStr");
            textBoxSendTime.Text = Config.GetValue("MailSendTime");
            Runtime.ShowLog("收件人地址:" + Config.GetValue("MailToStr"));
            Config.log.Info("收件人地址:" + Config.GetValue("MailToStr"));

            Runtime.ShowLog("抄送地址:" + Config.GetValue("MailToCcStr"));
            Config.log.Info("抄送地址:" + Config.GetValue("MailToCcStr"));

            Runtime.ShowLog("定时发送时间为:" + Config.GetValue("MailSendTime"));
            Config.log.Info("定时发送时间为:" + Config.GetValue("MailSendTime"));
            btnStart.Enabled = false;
            btnStart.Text = "服务已运行";
            btnStart.BackColor = Color.Lime;
        }

        /// <summary>
        /// 发送测试邮件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestSendMail_Click(object sender, EventArgs e)
        {
            //SentReportJob sentReport = new SentReportJob();
            //await sentReport.Execute(null);

            //return Task.Run(async () =>
            //{

            //    Console.WriteLine("定时任务执行:" + DateTime.Now.ToString());
            //});


            string urlApi = "http://47.100.19.132:8060/appzhwater/getDyhMsg?SYNC_KEY=FSR3RFDAFW445452";
            //string jsonArray = GetWebAPI.GetResponse(urlApi,out string statusCode);
            string jsonArrayStr = GetWebAPI.HttpGetApi(urlApi, out string statusCode);


            Console.WriteLine("statusCode:" + statusCode);
            Console.WriteLine("获取Json:" + jsonArrayStr);
            if (statusCode == "OK")
            {
                JArray jArray = JArray.Parse(jsonArrayStr);
                foreach (var item in jArray)
                {
                    JObject jObj = JObject.Parse(item.ToString());


                    Console.WriteLine("市政表编号:" + jObj["METER_NO"]);
                    Console.WriteLine("表名称:" + jObj["METER_NAME"]);
                    Console.WriteLine("瞬时流量:" + jObj["TIME_VALUE"]);
                    Console.WriteLine("瞬时流量更新时间:" + jObj["UPDATE_TIME_DATE"]);
                    Console.WriteLine("累计流量:" + jObj["READ_VALUE"]);
                    Console.WriteLine("累计流量更新时间:" + jObj["UPDATE_READ_DATE"]);


                    using (SqlConnection conn = new SqlConnection(Runtime.MSSQLServerConnect))
                    {
                        string sql = @"IF EXISTS (SELECT 1 FROM T_OV_MeterCurrentValue
                                                        WHERE METER_NO = @METER_NO
								                        )
					                        BEGIN

                                                UPDATE T_OV_MeterCurrentValue SET
                                                    METER_NAME = @METER_NAME,
                                                    TIME_VALUE = @TIME_VALUE,
                                                    UPDATE_TIME_DATE = @UPDATE_TIME_DATE,
                                                    READ_VALUE = @READ_VALUE,
                                                    UPDATE_READ_DATE = @UPDATE_READ_DATE
                                                    WHERE METER_NO = @METER_NO
                                            END

                                        ELSE

                                            BEGIN
                                                INSERT T_OV_MeterCurrentValue(
                                                    METER_NO, METER_NAME, TIME_VALUE,
						                            UPDATE_TIME_DATE, READ_VALUE, UPDATE_READ_DATE )
						                            VALUES
                                                    (@METER_NO, @METER_NAME, @TIME_VALUE,
                                                     @UPDATE_TIME_DATE, @READ_VALUE, @UPDATE_READ_DATE
                                                    )
                                            END ";

                        int count = conn.Execute(sql, new
                        {
                            METER_NO = jObj["METER_NO"].ToString(),
                            METER_NAME = jObj["METER_NAME"].ToString(),
                            TIME_VALUE = jObj["TIME_VALUE"].ToString(),
                            UPDATE_TIME_DATE = jObj["UPDATE_TIME_DATE"].ToString(),
                            READ_VALUE = jObj["READ_VALUE"].ToString(),
                            UPDATE_READ_DATE = jObj["UPDATE_READ_DATE"].ToString()
                        });

                        if (count > 0)
                        {
                            Console.WriteLine(count + "条操作成功");
                        }
                    }
                }


            }

        }


        //------------------窗体最小化，不退出软件----------------------------
        #region 窗体最小化，不退出软件
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            //窗体关闭原因为单击"关闭"按钮或Alt+F4  
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;           //取消关闭操作 表现为不关闭窗体  
                this.Hide();               //隐藏窗体  
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            //点击鼠标"左键"发生  
            if (e.Button == MouseButtons.Left)
            {
                this.Visible = true;                        //窗体可见  
                this.WindowState = FormWindowState.Normal;  //窗体默认大小  
                this.notifyIcon1.Visible = true;            //设置图标可见  
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //点击鼠标"左键"发生  
            if (e.Button == MouseButtons.Left)
            {
                this.Visible = true;                        //窗体可见  
                this.WindowState = FormWindowState.Normal;  //窗体默认大小  
                this.notifyIcon1.Visible = true;            //设置图标可见  
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Show();                                //窗体显示  
            this.WindowState = FormWindowState.Normal;  //窗体状态默认大小  
            this.Activate();                            //激活窗体给予焦点 
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            //点击"是(YES)"退出程序  
            if (MessageBox.Show("确定要退出程序?", "重要提示",
                        System.Windows.Forms.MessageBoxButtons.YesNo,
                        System.Windows.Forms.MessageBoxIcon.Warning)
                == System.Windows.Forms.DialogResult.Yes)
            {
                notifyIcon1.Visible = false;   //设置图标不可见  
                this.Close();                  //关闭窗体  
                this.Dispose();                //释放资源  
                Application.Exit();            //关闭应用程序窗体  
            }
        }

        #endregion 窗体最小化，不退出软件


    }
}
