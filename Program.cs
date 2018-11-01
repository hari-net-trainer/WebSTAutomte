using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace STAutomate
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ST Automate is starting...");
            Console.WriteLine("Please Enter user ID...");

            var userId = Console.ReadLine();
            Console.WriteLine("Please Enter password...");
            var password = Console.ReadLine();

            Console.WriteLine("work starting for user/password: " + userId + "/" + password);

            var driverService = FirefoxDriverService.CreateDefaultService();
            driverService.FirefoxBinaryPath = @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe";
            driverService.HideCommandPromptWindow = true;
            driverService.SuppressInitialDiagnosticInformation = true;

            var driverOptions = new FirefoxOptions();

            IWebDriver driverFF = null;
            try
            {
                CookieContainer cc = new CookieContainer();
                string internalUserId = string.Empty;

                var url = "https://www.socialtrade.biz";
                //var url = "http://sserp.ablazeerp.com";
                var urlViewWork = "/User/MyePoints.aspx";
                var urlGetWork = url + urlViewWork + "/GetWorkHistory";
                var urlUpdateLink = url + urlViewWork + "/UpdateTaskWork";
                var userHdn = "hfHiddenFieldID";

                using (driverFF = new FirefoxDriver(driverService, driverOptions, TimeSpan.FromSeconds(120)))
                {
                    driverFF.Navigate().GoToUrl(url + "/Default.aspx");

                    while (driverFF.PageSource.Contains("txtEmailID") == false)
                    {
                        Thread.Sleep(500);
                    }

                    var txt = driverFF.FindElement(By.Id("txtEmailID"));
                    var pwd = driverFF.FindElement(By.Id("txtPassword"));
                    var btn = driverFF.FindElement(By.Id("CndSignIn"));

                    txt.SendKeys(userId);
                    pwd.SendKeys(password);
                    btn.SendKeys(Keys.Enter);

                    while (driverFF.PageSource.Contains("View Advertisements") == false)
                    {
                        Thread.Sleep(500);
                    }

                    foreach (OpenQA.Selenium.Cookie c in driverFF.Manage().Cookies.AllCookies)
                    {
                        string name = c.Name;
                        string value = c.Value;
                        cc.Add(new System.Net.Cookie(name, value, c.Path, c.Domain));
                        Console.WriteLine("{0},{1},{2},{3}", name, value, c.Path, c.Domain);
                    }

                    driverFF.Navigate().GoToUrl(url + urlViewWork);

                    while (driverFF.PageSource.Contains(userHdn) == false)
                    {
                        Thread.Sleep(500);
                    }

                    internalUserId = driverFF.FindElement(By.Id(userHdn)).GetAttribute("value");
                    var wrokPayLoadObj = new WorkPayLoad { userId = internalUserId };
                    var postData = JsonConvert.SerializeObject(wrokPayLoadObj);
                    var resObj = HttpPostRequest<Rootobject>(urlGetWork, cc, postData);

                    if (resObj.d.AlertMsg == "CREATEGRID")
                    {
                        Console.WriteLine("Work Started....");
                        var count = resObj.d.TodayTaskLists.Count(x => x.Stage == "A" && x.CampaignTypeID == "4");
                        Console.WriteLine("No of Click(s) pending : " + count.ToString());
                    }

                    foreach (var item in resObj.d.TodayTaskLists)
                    {
                        var workUpdate = new UserPayLoad();
                        workUpdate.Username = internalUserId;

                        if (item.Stage == "A" && item.CampaignTypeID == "4")
                        {
                            #region Worked code
                            workUpdate.WorkID = Convert.ToInt32(item.WorkID);
                            workUpdate.CurrentFlag = "hand";
                            workUpdate.PointsType = 125;
                            workUpdate.Password = Convert.ToInt32(item.CampaignID);
                            var currentSNo = Convert.ToInt32(item.Serialnumber);
                            var nxtSNo = currentSNo + 1;
                            var nxtItem = resObj.d.TodayTaskLists.FirstOrDefault(x => x.Serialnumber == nxtSNo.ToString());
                            if (nxtItem != null)
                            {
                                if (nxtItem.Stage == "A" && nxtItem.CampaignTypeID == "4")
                                {
                                    workUpdate.Flag = "hand";
                                    workUpdate.NextWorkID = nxtItem.WorkID;
                                }
                            }
                            SHDocVw.InternetExplorer IE = null;
                            try
                            {
                                IE = new SHDocVw.InternetExplorer();
                                object URL = item.Link;
                                IE.ToolBar = 0;
                                IE.StatusBar = false;
                                IE.MenuBar = false;
                                IE.Width = 622;
                                IE.Height = 582;
                                IE.Visible = false;
                                IE.Navigate2(ref URL);
                                Thread.Sleep(30000);
                                IE.Quit();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                Thread.Sleep(30000);
                                if (IE != null)
                                {
                                    IE.Quit();
                                }
                            }

                            var postUser = JsonConvert.SerializeObject(workUpdate);
                            postUser = "{_user: " + postUser + "}";
                            var user = HttpPostRequest<ResponseObject>(urlUpdateLink, cc, postUser, true);

                            if (user.IsUpdate == true)
                            {
                                Console.WriteLine(item.Serialnumber + " : Clicked");
                                if (currentSNo % 15 == 0)
                                {
                                    driverFF.Navigate().Refresh();
                                }
                            }
                            #endregion
                        }
                    }
                    Console.WriteLine("Work Completed....");
                    Console.WriteLine("press any key to exit the ST Automate");
                    Console.ReadKey();
                    driverFF.Quit();
                }
            }
            catch (Exception exMain)
            {
                if (driverFF != null)
                {
                    driverFF.Quit();
                }
                Console.WriteLine("Work intrupted....");
                Console.WriteLine(exMain.Message);
                Console.WriteLine("press any key to exit the ST Automate");
                Console.ReadKey();
            }
        }

        private static T HttpPostRequest<T>(string url, CookieContainer cookieJar, string postData, bool isLastReq = false)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            var jsonTxt = string.Empty;
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            #region Request Headers
            //From chrome
            //Accept: text / html,application / xhtml + xml,application / xml; q = 0.9,image / webp,*/*;q=0.8
            //Accept-Encoding:gzip, deflate, br
            //Accept-Language:en-GB,en-US;q=0.8,en;q=0.6
            //Cache-Control:max-age=0
            //Connection:keep-alive
            //Content-Length:378
            //Content-Type:application/x-www-form-urlencoded
            //Cookie:ARRAffinity=803203185e4f6e0f5878e0e1224978852c5ac41eb5f747bc9c455223009c14bb; __asc=1969ca23158dc8c5b68309533b5; __auc=4c18b7ef158d3a871361a4d4d4a
            //Host:socialtrade.biz
            //Origin:https://socialtrade.biz
            //Referer:https://socialtrade.biz/login.aspx
            //Upgrade-Insecure-Requests:1
            //User-Agent:Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.75 Safari/537.36

            //From FireFox
            //Host: socialtrade.biz
            //User - Agent: Mozilla / 5.0(Windows NT 6.2; WOW64; rv: 50.0) Gecko / 20100101 Firefox / 50.0
            //Accept: application / json, text / javascript, */*; q=0.01
            //Accept-Language: en-GB,en;q=0.5
            //Accept-Encoding: gzip, deflate, br
            //Content-Type: application/json; charset=utf-8
            //X-Requested-With: XMLHttpRequest
            //Referer: https://socialtrade.biz/User/TodayTask179.aspx
            //Content-Length: 57
            //Cookie: __auc=21bde656158e93e701c13ee2a7e; 
            //ARRAffinity =7d4680109af4d9da8d787f468cb6e2e9aedf0108fc717b4591c7c8ce459417ed; 
            //__asc =52b2ad73158eca67dd1639a93f5; 
            //ASP.NET_SessionId=w2tqjsmahjx3fp3xnklzxonq; 
            //UserInfo =Name=Chavan Shiva krishna&UserID=596917&Expire=1 Days&Work=None&IP=&Date=&Time=&Status=A
            //Connection: keep-alive
            //Cache-Control: max-age=0
            #endregion

            myHttpWebRequest.Host = "socialtrade.biz";
            myHttpWebRequest.UserAgent = "Mozilla / 5.0(Windows NT 6.2; WOW64; rv: 50.0) Gecko / 20100101 Firefox / 50.0";
            myHttpWebRequest.Accept = "application / json, text / javascript, */*; q=0.01";
            myHttpWebRequest.ContentType = "application/json; charset=utf-8";
            //myHttpWebRequest.Referer = "https://socialtrade.biz/User/TodayTask179.aspx";

            myHttpWebRequest.CookieContainer = cookieJar;

            myHttpWebRequest.Method = "POST";

            //string postData = @"{'pageNumber':'1', 'pageSize': '20', 'userId': '596917'}";
            byte[] bytes = Encoding.UTF8.GetBytes(postData);
            myHttpWebRequest.ContentLength = bytes.Length;

            Stream requestStream = myHttpWebRequest.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);

            WebResponse response = myHttpWebRequest.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);

            jsonTxt = reader.ReadToEnd();
            stream.Dispose();
            reader.Dispose();

            if (isLastReq)
            {
                jsonTxt = jsonTxt.Replace("{\"d\":", string.Empty);
                jsonTxt = jsonTxt.Substring(0, jsonTxt.Length - 1);
            }

            T todaysTaskList = JsonConvert.DeserializeObject<T>(jsonTxt);
            return todaysTaskList;
        }

        public static string BuildHTML(Rootobject response)
        {
            //$("#hiddenPageNumber").val(pageNumber);
            var tblHeader = new StringBuilder("<table><thead><tr class='header_self' style='background-color: #192A32;'><th >Sr No.</th> <th >Page Title</th> <th >Status</th> <th >Action</th> </tr> </thead>");
            var customers = response.d.TodayTaskLists;
            var alertMsg = response.d;
            if (alertMsg.AlertMsg == "CREATEGRID")
            {
                //$("#reqfortodaywork").hide();
                //$("#mainDivHeader").html("");
                //$("#mainDivHeader").append(tblHeader);
                var enabledTr = 0;
                var tblBody = new StringBuilder();
                foreach (var item in customers)
                {
                    var stage = "";
                    var action = "";
                    var handIcon = "";
                    var refreshIcon = "";
                    var reportIcon = "";
                    var loaderIcon = "<span style='display:none;' id='loader_" + item.WorkID + "'><a href='javascript:void(0);'><i class='fa fa-spinner faa-spin animated'></i></span></a>";
                    if (item.CampaignTypeID == "4")
                    {
                        if (item.Stage == "C" || item.Stage == "D")
                        {
                            stage = "<span id='click_" + item.WorkID + "'><b class='clicked'>Clicked</b></span>";
                            handIcon = "<span  id='hand_" + item.WorkID + "' class='fade'><i class='fa fa-hand-o-up custom'></i></span>";
                        }
                        else
                        {
                            enabledTr = enabledTr + 1;
                            if (enabledTr < 2)
                            {
                                handIcon = "<span class='handIcon' title='Click Task' id='hand_" + item.WorkID + "' campaignid = " + item.CampaignID + "  link = '" + item.Link + "' onclick='updateTask(" + item.WorkID + ", item)' ><i class='fa fa-hand-o-up custom'>HAND</i></span>";
                                //handIcon = "<span class='handIcon' id='hand_" + item.WorkID + "' campaignid = '" + item.CampaignID + "' link = '" + item.Link + "' onclick='updateTask(" + item.WorkID + ", item)'><i class='fa fa-hand-o-up custom'></i></span>";
                                //reportIcon = "<span class='handIcon' title='Report Abuse' id='report_" + item.WorkID + "' campaignid = " + item.CampaignID + " onclick='reportTask(" + item.WorkID + ", item)' ><i  style='font-size:18px !important;' class='fa fa-exclamation-triangle' aria-hidden='true'></i></span>";
                                refreshIcon = "<span class='handIcon' title='Refresh' id='refresh_" + item.WorkID + "' link = " + item.Link + " onclick='refreshTask(" + item.WorkID + ", item)'><i class='fa fa-refresh' aria-hidden='true'>ReFresh</i></span>";
                            }
                            else
                            {
                                handIcon = "<span id='hand_" + item.WorkID + "'  campaignid = '" + item.CampaignID + "' link = '" + item.Link + "'><i class='fa fa-hand-o-up custom'></i></span>";
                            }
                        }
                    }
                    else if (item.CampaignTypeID == "1")
                    {
                        if (item.Stage == "C" || item.Stage == "D")
                        {
                            stage = "<span id='click_" + item.WorkID + "'><b class='clicked'>Clicked</b></span>";
                            handIcon = "<span  id='facebook_" + item.WorkID + "' class='fade'><i class='fa fa-facebook'></i></span>";
                        }
                        else
                        {
                            enabledTr = enabledTr + 1;
                            if (enabledTr < 2)
                            {
                                handIcon = "<span id='facebook_" + item.WorkID + "' campaignid = '" + item.CampaignID + "' link = '" + item.Link + "'><div class='fb-like' style='padding: 6px;' data-href=" + item.Link + "###" + item.WorkID + " data-layout='button' data-action='like' data-show-faces='false' data-share='false'></div></span>";
                                //handIcon = "<span class='handIcon' id='facebook_" + item.WorkID + "' link = " + item.Link + " onclick='updateTask(" + item.WorkID + ",item)'><i class='fa fa-facebook'></i></span>";
                                // reportIcon = "<span class='handIcon' title='Report Abuse' id='report_" + item.WorkID + "' campaignid = " + item.CampaignID + " onclick='reportTask(" + item.WorkID + ", item)' ><i style='font-size:18px !important;' class='fa fa-exclamation-triangle' aria-hidden='true'></i></span>";
                                refreshIcon = "<span class='handIcon' title='Refresh' id='refresh_" + item.WorkID + "' link = " + item.Link + " onclick='refreshTask(" + item.WorkID + ", item)'><i class='fa fa-refresh' aria-hidden='true'></i></span>";
                            }
                            else
                            {
                                handIcon = "<span id='facebook_" + item.WorkID + "' campaignid = '" + item.CampaignID + "' link = '" + item.Link + "'><i class='fa fa-facebook'></i></span>";
                            }
                        }
                    }
                    if (item.Stage == "C")
                    {
                        stage = "<span id='click_" + item.WorkID + "'><b class='clicked'>Clicked</b></span>";
                    }
                    else if (item.Stage == "D")
                    {
                        stage = "<span id='click_" + item.WorkID + "'><b class='Donation'>Clicked</b></span>";
                    }
                    else if (item.Stage == "A")
                    {
                        stage = "<span id='pending_" + item.WorkID + "'><b class='pending'>Pending</b></span>";
                    }
                    // reportIcon = "<span class='handIcon' title='Report Abuse' id='report_" + item.WorkID + "' campaignid = " + item.CampaignID + " onclick='reportTask(" + item.WorkID + ", item)' ><i style='font-size:18px !important;' class='fa fa-exclamation-triangle' aria-hidden='true'></i></span>";
                    action = loaderIcon + "&nbsp;" + action + "&nbsp;" + handIcon + "&nbsp;" + refreshIcon + "&nbsp;" + reportIcon;
                    var tblRow = "<tr class='row_self'><td id='noborder'>" + item.Serialnumber + "</td><td id='camName_" + item.WorkID + "'  class='camp_name'><a target='_blank' href='" + item.Link + "'>" + item.CampaignName + "</a></td><td id='noborder'>" + stage + "</td><td class='noborder' id= 'action_" + item.WorkID + "'>" + action + "</td></tr>";
                    tblBody = tblBody.Append(tblRow);
                }
                tblHeader = tblHeader.Append("<tbody>").Append(tblBody).Append("</tbody></table>");
            }

            return tblHeader.ToString();
        }


    }

}
