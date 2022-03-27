using Dapper;
using exchangeRate.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;


namespace exchangeRate.Controllers
{
    public class GetRateController : ApiController
    {
        /// <summary>
        /// 連線字串
        /// </summary>
        private readonly string _connectString = @"Persist Security Info=False;Trusted_Connection=True;database=exchangeRate;server=(local)";

        public IEnumerable<exchangeRatesVM> GetList()
        {
            using (var conn = new SqlConnection(_connectString))
            {
                var result = conn.Query<exchangeRatesVM>("SELECT * FROM exchangeRates");
                return result;
            }
        }

        public Boolean UpdateList(string currencyId, string currencyName)
        {
            using (var conn = new SqlConnection(_connectString))
            {
                string sql = @"UPDATE exchangeRates
                                SET currencyId=@currencyId, currencyName=@currencyName
                                WHERE currencyId=@currencyId;";
                //設定參數
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@currencyId", currencyId, DbType.String, ParameterDirection.Input);
                parameters.Add("@currencyName", currencyName, DbType.String, ParameterDirection.Input);
                var result = conn.Execute(sql, parameters);
                return true;
            }
        }

        public Boolean InsertList(string currencyId, string currencyName)
        {
            using (var conn = new SqlConnection(_connectString))
            {
                string sql = @"INSERT INTO exchangeRates 
                                        (currencyId, currencyName,cashSale,cashBuy,lastUpdateTime)
                                 VALUES (@currencyId, @currencyName,0,0,'');";
                //設定參數
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@currencyId", currencyId, DbType.String, ParameterDirection.Input);
                parameters.Add("@currencyName", currencyName, DbType.String, ParameterDirection.Input);
                var result = conn.Execute(sql, parameters);
                return true;
            }
        }

        public void DeleteList(string currencyId)
        {
            using (var conn = new SqlConnection(_connectString))
            {
                string sql = @"DELETE FROM exchangeRates
                                WHERE currencyId = @currencyId;";
                //設定參數
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@currencyId", currencyId, DbType.String, ParameterDirection.Input);
                conn.Execute(sql, parameters);
            }
        }

        // GET api/values
        public IEnumerable<string> Get()
        {
            #region 設定局部CORS

            // *代表任何網域資源都接收，不建議用*
            HttpContext.Current.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            HttpContext.Current.Response.AppendHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
            HttpContext.Current.Response.AppendHeader("Access-Control-Allow-Methods", "GET,PUT,POST,DELETE");

            #endregion
            return new string[] { "value1", "value2" };
        }

        // POST api/values
        public dynamic Post([FromBody] dynamic value)
        {
            //取得幣別列表
            if (value?.currencyId == null && value?.exchangeDate == null)
            {
                List<currencyVM> ResList = new List<currencyVM>();
                var res = GetList();
                res.ForEach(x =>
                {
                    currencyVM Res = new currencyVM();
                    Res.currencyId = x.currencyId;
                    Res.currencyName = x.currencyName;
                    ResList.Add(Res);
                });

                return ResList;
            }

            // 新增&修改幣別
            if (value?.currencyId != null && value?.currencyName != null)
            {
                bool checkExist = true;
                var res = GetList();
                // 修改
                res.ForEach(x =>
                {
                    if (value?.currencyId == x.currencyId.Trim())
                    {
                        checkExist = false;
                    }
                });
                if (!checkExist)
                {
                    string UpdateCurrencyId = value.currencyId;
                    string UpdateCurrencyName = value.currencyName;
                    UpdateList(UpdateCurrencyId, UpdateCurrencyName);
                } 
                // 新增
                if (checkExist)
                {
                    string UpdateCurrencyId = value.currencyId;
                    string UpdateCurrencyName = value.currencyName;
                    InsertList(UpdateCurrencyId, UpdateCurrencyName);
                }

                return true;
            }

            // 刪除幣別
            if (value?.currencyId != null && value?.currencyName == null)
            {
                int removwIndex = 0;
                bool removeFlag = false;
                var res = GetList();
                res.ForEach(x =>
                {
                    if (value?.currencyId == x.currencyId.Trim())
                    {
                        removeFlag = true;
                    }
                });
                if (removeFlag) 
                {
                    string UpdateCurrencyId = value.currencyId;
                    DeleteList(UpdateCurrencyId);
                }
                return true;
            }

            // 取得某日匯率
            if (value?.exchangeDate != null)
            {
                try
                {
                    var res = GetList();
                    string date = value.exchangeDate;
                    date = date.Replace("/", "");

                    List<exchangeRatesVM> ResList = new List<exchangeRatesVM>();

                    res.ForEach(x =>
                    {
                        if (x.lastUpdateTime.Substring(0, 8) == date) ResList.Add(x);
                    });

                    if (ResList.Count == 0) return "查無資料";
                    return ResList;
                }
                catch
                {
                    return "輸入有誤";
                }

            }
            return value;
        }
    }
}
