using exchangeRate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace exchangeRate.Controllers
{
    public class GetRateController : ApiController
    {
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
            #region 假資料
            List<exchangeRatesVM> exchangeRatesDataList = new List<exchangeRatesVM>();
            exchangeRatesDataList.Add(new exchangeRatesVM()
            {
                currencyId = "USD",
                currencyName = "美金",
                cashSale = 29.4580,
                cashBuy = 29.3580,
                lastUpdateTime = "20200606150000"
            });

            exchangeRatesDataList.Add(new exchangeRatesVM()
            {
                currencyId = "JYP",
                currencyName = "日圓",
                cashSale = 0.2752,
                cashBuy = 0.2712,
                lastUpdateTime = "20200606150000"
            });

            List<currencyVM> currencyDataList = new List<currencyVM>();
            currencyDataList.Add(new currencyVM()
            {
                currencyId = "TWD",
                currencyName = "台幣",
            });
            currencyDataList.Add(new currencyVM()
            {
                currencyId = "USD",
                currencyName = "美金",
            });
            #endregion

            //取得幣別列表
            if (value?.currencyId == null && value?.exchangeDate == null)
            {
                List<currencyVM> ResList = new List<currencyVM>();

                currencyDataList.ForEach(x =>
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
                int index = 0;
                // 修改
                currencyDataList.ForEach(x =>
                {
                    if (value?.currencyId == x.currencyId)
                    {
                        currencyDataList[index].currencyName = value.currencyName;
                        checkExist = false;
                    }
                    index++;
                });
                // 新增
                if (checkExist)
                {
                    currencyDataList.Add(new currencyVM()
                    {
                        currencyId = value.currencyId,
                        currencyName = value.currencyName
                    });
                }

                return currencyDataList;
            }

            // 刪除幣別
            if (value?.currencyId != null && value?.currencyName == null)
            {
                int removwIndex = 0;
                bool removeFlag = false;
                int index = 0;
                // 修改
                currencyDataList.ForEach(x =>
                {
                    if (value?.currencyId == x.currencyId)
                    {
                        removwIndex = index;
                        removeFlag = true;
                    }
                    index++;
                });
                if(removeFlag) currencyDataList.RemoveAt(removwIndex);
                return currencyDataList;
            }

            // 取得某日匯率
            if (value?.exchangeDate != null)
            {
                try
                {
                    string date = value.exchangeDate;
                    date = date.Replace("/", "");

                    List<exchangeRatesVM> ResList = new List<exchangeRatesVM>();

                    exchangeRatesDataList.ForEach(x =>
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
