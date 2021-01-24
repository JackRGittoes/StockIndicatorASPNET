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
using System.Net;
using System.IO;
using System.Text;

namespace StockIndicator.Web.Controllers
{
    public class StockCheckerController : Controller
    {
        static readonly HttpClient client = new HttpClient();
            
        static readonly HtmlDocument htmlDoc = new HtmlDocument();

        #region view

        public IActionResult Index()
        {
            return View(new StockCheckerModel());
        }
        public IActionResult Stop()
        {
            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddDays(-1);
            
            return View("Index", new StockCheckerModel());
        }

        public async Task<IActionResult> Start(StockCheckerModel model)
        {
            var urlKey = "URL";
            var sleepKey = "SLEEP";
            var urls = new List<string>();

            if (model.NoOfUrls == 0)
            {
                model.NoOfUrls = Convert.ToInt32(Request.Cookies["NoOfUrls"]);
                model.SleepTime = Request.Cookies[sleepKey];
            }
            else
            {             
                Cookies(model.URLS, model.SleepTime);
            }

            for (int i = 0; i < model.NoOfUrls; i++)
            {
                var cookieResult = Request.Cookies[$"{urlKey}{i}"];
                if (cookieResult != null)
                {
                    urls.Add(cookieResult);
                }
            }
            if (urls.Count >= 1)
            {
                model.URLS = urls;                 
            }
            await InStockAsync(model, model.URLS);
            return View("Index", model);
        }
        #endregion

        #region Cookies
        public void Cookies(List<String> urls, string sleepTime)
        {
            var urlKey = "URL";
            var sleepKey = "SLEEP";

            CookieOptions option = new CookieOptions();
            try
            {
                for (int i = 0; i < urls.Count; i++)
                {
                    Response.Cookies.Append($"{urlKey}{i}", urls[i], option);
                }
                Response.Cookies.Append(sleepKey, sleepTime.ToString(), option);
                Response.Cookies.Append("NoOfUrls", urls.Count.ToString(), option);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        #endregion

        public static string WebResponse(string url)
        {
            string data = null;
            var status = false;

            while (status == false)
            {
                for (int i = 0; i <= 5; i++)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Headers["user-agent"] = "	Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_5)AppleWebKit / 605.1.15(KHTML, like Gecko)Version / 12.1.1 Safari / 605.1.15";
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Stream receiveStream = response.GetResponseStream();
                        StreamReader readStream = null;

                        if (String.IsNullOrWhiteSpace(response.CharacterSet))
                            readStream = new StreamReader(receiveStream);
                        else
                            readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                        data = readStream.ReadToEnd();

                        response.Close();
                        readStream.Close();
                    }

                    if(data != null)
                    {
                        status = true;
                        break;
                    }
                    if(i == 5)
                    {
                        status = true;
                    }
                }             
            }
            return data;
        }


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
                var html = WebResponse(url);
                if (html != null)
                {
                    htmlDoc.LoadHtml(html);
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
                    catch (WebException ex)
                    {
                        Console.WriteLine(ex);
                    }
                    IsTrue = true;

                }
                if (retailer.Contains("argos") || retailer.Contains("game") || retailer.Contains("smyths"))
                {
                    return true;
                }
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

