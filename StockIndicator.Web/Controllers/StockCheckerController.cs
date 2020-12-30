using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockIndicator.Web.Models;
using System.Threading;
using System.Net.Http;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;

namespace StockIndicator.Web.Controllers
{
    public class StockCheckerController : Controller
    {
        static readonly HttpClient client = new HttpClient();
        static readonly HtmlDocument htmlDoc = new HtmlDocument();

        #region view
        public async Task<IActionResult> IndexAsync(StockCheckerModel model)
        {
            var urlKey = "URL";
            var sleepKey = "SLEEP";
            

            CookieOptions option = new CookieOptions();
            if (model.URLS != null && model.SleepTime != null)
            {
                for (int i = 0; i < model.URLS.Count; i++)
                {
                    Response.Cookies.Append($"{urlKey}{i}", model.URLS[i], option);
                }
                Response.Cookies.Append(sleepKey, model.SleepTime.ToString(), option);
                Response.Cookies.Append("NoOfUrls", model.URLS.Count.ToString(), option);
            }

            if (model.NoOfUrls == 0)
            {
                model.NoOfUrls = Convert.ToInt32(Request.Cookies["NoOfUrls"]);
            }

            if (model.SleepTime == null)
            {
                model.SleepTime = Request.Cookies[sleepKey];
                var urls = new List<string>();
                for (int i = 0; i < model.NoOfUrls; i++)
                {
                    var result = Request.Cookies[$"{urlKey}{i}"];
                    if (result != null)
                    {
                        urls.Add(result);
                    }
                 }
                if (urls.Count == 0)
                {

                }
                else
                {
                    model.URLS = urls;
                }
            }

           
            
           await InStockAsync(model, model.URLS);
           
            return View(model);
        }

        #endregion

        #region Stock Checker logic

        [HttpPost]
        public static async Task<bool> InStockAsync(StockCheckerModel model, List<string> urls)
        {
            var result = false;
            var results = new List<bool>();
            var retailers = new List<string>();
            var inStock = new List<bool>();

            try
            {
                for (int i = 0; i < urls.Count; i++)
                {
                    var retailer = WhatRetailer(urls[i]);
                    result = await StockCheckerAsync(urls[i], retailer);
                    retailers.Add(retailer);
                    results.Add(result);
                }

                if (result == true)
                {
                    inStock.Add(true);

                }
                else
                {
                    inStock.Add(false);
                }
                model.Retailers = retailers;
                model.Results = results;
                model.InStock = inStock;
            }
            catch(NullReferenceException ex)
            {
                Console.WriteLine(ex);
            }

            return true;
        }


        public static string WhatRetailer(string url)
        {
            var retailer = " ";
            if (url.ToLower().Contains("currys"))
            {
                
                retailer = "currys";
            }
            else if (url.ToLower().Contains("argos"))
            {
                retailer = "argos";
            }
            else if (url.ToLower().Contains("amazon"))
            {
                retailer = "amazon";

            }
            else if (url.ToLower().Contains("game.co.uk"))
            {
                retailer = "game";
            }
            else if (url.ToLower().Contains("scan"))
            {
                retailer = "scan";
            }
            else if (url.ToLower().Contains("smyths"))
            {
                retailer = "smyths";
            }
            return retailer;
        }

        public static async Task<bool> StockCheckerAsync(string url, string retailer)
        {
            var node = " ";
            var nodeContains = " ";
            if (retailer.Contains("currys"))
            {
                node = StockCheckerModel.currysNode;
                nodeContains = StockCheckerModel.currysNodeContains;
            }
            else if (retailer.Contains("argos"))
            {
                node = StockCheckerModel.argosNode;
                nodeContains = StockCheckerModel.argosNodeContains;
            }
            else if (retailer.Contains("amazon"))
            {
                node = StockCheckerModel.amazonNode;
                nodeContains = StockCheckerModel.amazonNodeContains;
            }
            else if (retailer.Contains("game"))
            {
                node = StockCheckerModel.gameNode;
                nodeContains = StockCheckerModel.gameNodeContains;
            }
            else if (retailer.Contains("scan"))
            {
                node = StockCheckerModel.scanNode;
                nodeContains = StockCheckerModel.scanNodeContains;
            }
            else if (retailer.Contains("smyths"))
            {
                node = StockCheckerModel.smythsNode;
                nodeContains = StockCheckerModel.smythsNodeContains;
            }


            bool IsTrue = false;
            while (IsTrue == false)
            {
                var responseBody = await client.GetStringAsync(url);
                htmlDoc.LoadHtml(responseBody);

                try
                {
                    foreach (var item in htmlDoc.DocumentNode.SelectNodes(node))
                    {

                        //If the retailer is argos returns false because we are tracking an out of stock text element
                        if (item.InnerText.Contains(nodeContains) && retailer.Contains("argos") || item.InnerText.Contains(nodeContains) && retailer.Contains("game") || item.InnerText.Contains(nodeContains) && retailer.Contains("smyths"))
                        {
                            return false;
                        }
                        else if (item.InnerText.Contains(nodeContains))
                        {
                            return true;
                        }

                    };
                }
                catch (NullReferenceException)
                {
                    if (retailer.Contains("argos") || retailer.Contains("game") || retailer.Contains("smyths"))
                    {
                        return true;
                    }
                    return false;
                }
                IsTrue = true;
            }
            if (retailer.Contains("argos") || retailer.Contains("game") || retailer.Contains("smyths"))
            {
                return true;
            }
            return false;
        }

        public static int SleepTimer()
        {
            int sleepTime;
            try
            {
                Console.WriteLine("Input in Ms how often you want to check for stock (e.g. 5000 = 5 seconds)\nMinimum 5 seconds");
                sleepTime = Convert.ToInt32(Console.ReadLine());

            }
            catch (FormatException)
            {
                Console.WriteLine("Defaulted to 5 seconds");
                return 5000;
            }
            if (sleepTime < 5000)
            {
                Console.WriteLine("Defaulted to 5 seconds");
                return 5000;
            }
            return sleepTime;

        }

        public static List<String> GetURL()
        {
            var noOfUrls = 0;
            List<string> urls = new List<string>();
            bool input = false;
            while (input == false)
            {
                try
                {
                    Console.WriteLine("\nHow many URL's do you want to track");
                    noOfUrls = Convert.ToInt32(Console.ReadLine());
                    if (noOfUrls >= 1)
                    {
                        input = true;
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid Input");
                }
            }

            for (int i = 0; i < noOfUrls; i++)
            {
                var counter = i + 1;
                Console.WriteLine("\nInput URL " + counter + ": ");
                var url = Console.ReadLine();
                urls.Add(url);
            }
            return urls;
        }
        #endregion
    }
}

