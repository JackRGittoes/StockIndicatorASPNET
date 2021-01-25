using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockIndicator.Web.Models
{
    public class StockCheckerModel
    {

        public const string amazonNode = "//span[@id='submit.add-to-cart-announce']";
        public const string argosNode = "//strong";
        public const string currysNode = "//div";
        public const string gameNode = "//div";
        public const string scanNode = "//div";
        public const string smythsNode = "//p[@class=' deliveryType homeDelivery js-stockStatus']";

        public const string amazonNodeContains = "Add to Basket";
        public const string argosNodeContains = "Not available";
        public const string currysNodeContains = "Add to basket";
        public const string gameNodeContains = "out of stock";
        public const string scanNodeContains = "In stock";
        public const string smythsNodeContains = "In Stock";


        public string Message { get; set; }
        public List<string> URLS { get; set; }
        public string SleepTime { get; set; }
        public List<bool> InStock { get; set; }
        public List<string> Retailers { get; set; }
        public bool OOS { get; set; }
        public int NoOfUrls{ get; set;  }
        public List<string> UrlCookies { get; set; }
        public List<bool> Results { get; set; }
        public bool Stop { get; set; }
    }
}
