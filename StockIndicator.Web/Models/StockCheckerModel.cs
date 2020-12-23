using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockIndicator.Web.Models
{
    public class StockCheckerModel
    {

        public const string amazonNode = "//div";
        public const string argosNode = "//strong";
        public const string currysNode = "//div";
        public const string gameNode = "//div";
        public const string scanNode = "//div";
        public const string smythsNode = "//div";

        public const string amazonNodeContains = "Add to Basket";
        public const string argosNodeContains = "Not available";
        public const string currysNodeContains = "Add to basket";
        public const string gameNodeContains = "out of stock";
        public const string scanNodeContains = "In stock";
        public const string smythsNodeContains = "Out of Stock";


        public string Message { get; set; }
        public string URL { get; set; }
        public string SleepTime { get; set; }
        public bool InStock { get; set; }
        public string Retailer { get; set; }
        public bool OOS { get; set; }
    }
}
