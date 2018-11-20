using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace EmailService.Common
{
    public class ProcessState
    {
        public string ProcessName { get; set; }
        public int State { get; set; }
        public string UpdateTime { get; set; }

      
        public void TestMath() { Config.log.Info("**Test()方法执行 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); }
        /// <summary>
        /// 获取进程运行状态 并将当前运行状态写入数据库
        /// </summary>
        /// <returns></returns>
        public List<ProcessState> GetProcessState()
        {
            
            List<ProcessState> ProcessStates = new List<ProcessState>();

            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //监测的进程名称
            string CheckProcessName = Config.GetValue("CheckProcessName");

            string[] CheckProcessArry = CheckProcessName.Split(';');
        
            foreach (var pName in CheckProcessArry)
            {
                ProcessState pState = new ProcessState();
                int RunState = 0;
                if (Process.GetProcessesByName(pName).ToList().Count > 0)
                {
                    //运行
                    RunState = 1;
                }
                else
                {
                    //不运行
                    //RunState = 0;
                }

                pState.ProcessName = pName;
                pState.State = RunState;
                pState.UpdateTime = time;
                ProcessStates.Add(pState);

                //当前进程的运行状态存入数据库
                string sql = @"INSERT INTO ProcessState ( ProcessName,State,UpdateTime)
                                        VALUES(@ProcessName, @State,@UpdateTime); ";

                SQLiteParameter[] parameters =  {
                        new SQLiteParameter("@ProcessName",pName),
                        new SQLiteParameter("@State",RunState),
                        new SQLiteParameter("@UpdateTime",time)
                 };

                //countSql++;
                //Config.log.Info("**循环次数 " + countSql+ "   tiem="+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")); 
                //Console.WriteLine("**循环次数 " + countSql+ "   tiem="+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                int sqlResult = SqliteHelper.ExecuteNonQuery(sql, parameters);
                if (sqlResult > 0)
                {
                    Config.log.Info("**进程运行状态写入数据库完成，时间： " + time);
                }
                else
                {
                    Config.log.Info("!!进程运行状态写入数据库失败，时间： " + time);
                }

                //Runtime.ShowLog("sql执行结果： " + sqlResult + " 行");
            }

            //SQLiteDataReader reader = SqliteHelper.ExecuteReader("SELECT * FROM ProcessState", null);

            //while (reader.Read())
            //{
            //    Runtime.ShowLog("查询结果：" + " 进程名称=" + reader["ProcessName"] + "; 运行状态=" + reader["State"].ToString() + "; 检查时间=" + reader["UpdateTime"].ToString());
            //}

            return ProcessStates;
        }

    }
}
