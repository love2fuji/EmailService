using Dapper;
using EmailService.Common;
using Newtonsoft.Json.Linq;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EmailService.WorkJob
{
    public class GetWebAPI : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            string UpdateSQL = @"IF EXISTS (SELECT 1 FROM T_OV_MeterCurrentValue
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

            string InsertLogSQL = @"
                    INSERT T_OV_MeterValueLog(
                           METER_NO, METER_NAME, TIME_VALUE,
						   UPDATE_TIME_DATE, READ_VALUE, UPDATE_READ_DATE )
						   VALUES
                           (@METER_NO, @METER_NAME, @TIME_VALUE,
                            @UPDATE_TIME_DATE, @READ_VALUE, @UPDATE_READ_DATE
                           )
                           ";

            try
            {
                Config.log.Info("------开始 请求webApi数据 任务 ------");
                Runtime.ShowLog("------开始 请求webApi数据 任务 ------");

                //string ApiURL = "http://47.100.19.132:8060/appzhwater/getDyhMsg?SYNC_KEY=FSR3RFDAFW445452";

                string jsonArrayStr = GetWebAPI.HttpGetApi(Runtime.ApiURL, out string statusCode);

                Config.log.Info("------ 请求webApi数据 结果： statusCode：" + statusCode);
                Runtime.ShowLog("------ 请求webApi数据 结果： statusCode：" + statusCode);

                Config.log.Info("------ 获取原始Json数据 :" + jsonArrayStr);

                if (statusCode == "OK")
                {
                    Config.log.Info("------ 开始 解析数据 ------");
                    int count = 0;
                    JArray jArray = JArray.Parse(jsonArrayStr);
                    foreach (var item in jArray)
                    {
                        JObject jObj = JObject.Parse(item.ToString());
                        Config.log.Info("------ 开始 存储数据 ------");
                        using (SqlConnection conn = new SqlConnection(Runtime.MSSQLServerConnect))
                        {
                            int resultSQL = conn.Execute(UpdateSQL, new
                            {
                                METER_NO = jObj["METER_NO"].ToString(),
                                METER_NAME = jObj["METER_NAME"].ToString(),
                                TIME_VALUE = jObj["TIME_VALUE"].ToString(),
                                UPDATE_TIME_DATE = jObj["UPDATE_TIME_DATE"].ToString(),
                                READ_VALUE = jObj["READ_VALUE"].ToString(),
                                UPDATE_READ_DATE = jObj["UPDATE_READ_DATE"].ToString()
                            });

                            int logCount = conn.Execute(InsertLogSQL, new
                            {
                                METER_NO = jObj["METER_NO"].ToString(),
                                METER_NAME = jObj["METER_NAME"].ToString(),
                                TIME_VALUE = jObj["TIME_VALUE"].ToString(),
                                UPDATE_TIME_DATE = jObj["UPDATE_TIME_DATE"].ToString(),
                                READ_VALUE = jObj["READ_VALUE"].ToString(),
                                UPDATE_READ_DATE = jObj["UPDATE_READ_DATE"].ToString()
                            });

                            if (resultSQL > 0)
                            {
                                Config.log.Info("------ 完成 存储数据，影响行数：" + resultSQL);
                            }
                            else
                            {
                                Config.log.Error("------ 存储数据 失败！");
                            }

                            count = count + resultSQL;
                        }

                    }

                    Config.log.Info("------ 完成 解析数据 总共影响行数：" + count);
                    Runtime.ShowLog("------ 完成 解析数据 总共影响行数：" + count);
                }
                else
                {
                    Config.log.Info("------ 请求webApi 返回数据无效 ------");
                    Runtime.ShowLog("------ 请求webApi 返回数据无效 ------");
                }

                Config.log.Info("------ 完成 请求webApi数据 任务 ------");
                Runtime.ShowLog("------ 完成 请求webApi数据 任务 ------");

            }
            catch (Exception ex)
            {
                Config.log.Error("----- 请求webApi数据任务 失败！ 详细：" + ex.Message);
                Runtime.ShowLog("----- 请求webApi数据任务 失败！ 详细：" + ex.Message);
            }
            return Task.CompletedTask;
        }



        /// <summary>  
        /// 调用api返回json  
        /// </summary>  
        /// <param name="url">api地址</param>  
        /// <param name="jsonstr">接收参数</param>  
        /// <param name="type">类型</param>  
        /// <returns></returns>  
        public static string HttpApi(string url, string jsonstr, string type)
        {
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);//webrequest请求api地址  
            request.Accept = "text/html,application/xhtml+xml,*/*";
            request.ContentType = "application/json";
            request.Method = type.ToUpper().ToString();//get或者post  
            byte[] buffer = encoding.GetBytes(jsonstr);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        // Get请求
        public static string GetResponse(string url, out string statusCode)
        {
            string result = string.Empty;

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = httpClient.GetAsync(url).Result;
                statusCode = response.StatusCode.ToString();

                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsStringAsync().Result;
                }
            }
            return result;
        }

        public static string HttpGetApi(string url, out string statusCode)
        {
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            statusCode = response.StatusCode.ToString();

            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
