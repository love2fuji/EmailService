using EmailService.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
            btnStart.Text = "启动服务";
            btnStop.Text = "服务已停止";

            Runtime.ShowLog("初始化软件");
            Config.log.Info("初始化软件");
            txtBoxEmailAddress.Text = Config.GetValue("MailToStr"); 
            textBoxSendTime.Text = Config.GetValue("MailSendTime");
        }
    }
}
