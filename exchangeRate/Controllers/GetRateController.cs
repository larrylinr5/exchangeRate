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

        public IEnumerable<currencyVM> GetCurrency()
        {
            using (var conn = new SqlConnection(_connectString))
            {
                var result = conn.Query<currencyVM>("SELECT * FROM currency");
                return result;
            }
        }

        public IEnumerable<exchangeRatesVM> GetExchangeRates()
        {
            using (var conn = new SqlConnection(_connectString))
            {
                var result = conn.Query<exchangeRatesVM>("SELECT * FROM exchangeRates");
                return result;
            }
        }

        public Boolean UpdateCurrency(string currencyId, string currencyName)
        {
            using (var conn = new SqlConnection(_connectString))
            {
                string sql = @"UPDATE currency
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

        public Boolean InsertCurrency(string currencyId, string currencyName)
        {
            using (var conn = new SqlConnection(_connectString))
            {
                string sql = @"INSERT INTO currency 
                                        (currencyId, currencyName)
                                 VALUES (@currencyId, @currencyName);";
                //設定參數
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@currencyId", currencyId, DbType.String, ParameterDirection.Input);
                parameters.Add("@currencyName", currencyName, DbType.String, ParameterDirection.Input);
                var result = conn.Execute(sql, parameters);
                return true;
            }
        }

        public Boolean DeleteCurrency(string currencyId)
        {
            using (var conn = new SqlConnection(_connectString))
            {
                string sql = @"DELETE FROM currency
                                WHERE currencyId = @currencyId;";
                //設定參數
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@currencyId", currencyId, DbType.String, ParameterDirection.Input);
                conn.Execute(sql, parameters);
                return true;
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

        //取得幣別列表
        [HttpPost]
        [ActionName("getData")]
        public dynamic getData([FromBody] dynamic value) 
        {
            return GetCurrency();
        }

        // 新增幣別
        [HttpPost]
        [ActionName("createData")]
        public dynamic createData([FromBody] dynamic value)
        {
            bool checkExist = true;
            var res = GetCurrency();
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
                return "已存在此currencyId";
            }
            else
            {
                string UpdateCurrencyId = value.currencyId;
                string UpdateCurrencyName = value.currencyName;
                return InsertCurrency(UpdateCurrencyId, UpdateCurrencyName);
            }
        }

        // 修改幣別
        [HttpPost]
        [ActionName("updateData")]
        public dynamic updateData([FromBody] dynamic value)
        {
            bool checkExist = true;
            var res = GetCurrency();
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
                return UpdateCurrency(UpdateCurrencyId, UpdateCurrencyName);
            }
            else
            {
                return "無此資料可提供修改";
            }
        }

        // 刪除幣別
        [HttpPost]
        [ActionName("deleteData")]
        public dynamic deleteData([FromBody] dynamic value)
        {
            bool removeFlag = false;
            var res = GetCurrency();
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
                return DeleteCurrency(UpdateCurrencyId);
            }
            return "刪除失敗";
        }

        // 取得某日匯率
        [HttpPost]
        [ActionName("getExchangeData")]
        public dynamic getExchangeData([FromBody] dynamic value)
        {
            try
            {
                var res = GetExchangeRates();
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
    }
}
