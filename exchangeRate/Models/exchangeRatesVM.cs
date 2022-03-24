using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace exchangeRate.Models
{
    public class exchangeRatesVM
    {
        public string currencyId { get; set; }
        public string currencyName { get; set; }
        public double cashSale { get; set; }
        public double cashBuy { get; set; }
        public string lastUpdateTime { get; set; }
    }
}