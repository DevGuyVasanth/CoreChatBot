﻿using CoreBot.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace CoreBot.Model
{
    public class APIRequest
    {
        private static readonly ILogger Logger;

        public string CreateIncident(string desc, string emailid, string json, string url)
        {
            string incidentNumber = string.Empty;
            //string url = _iconfiguration.GetValue<string>("IncidentCreateURL"); //"https://hexawaredemo1.service-now.com/api/now/v1/table/incident";
            (bool, string) response = PostData(json, url);

            if (response.Item1 && !string.IsNullOrEmpty(response.Item2))
            {
                JObject jObj = JObject.Parse(response.Item2);
                incidentNumber = Convert.ToString(jObj.SelectToken("result.number"));
            }
            return incidentNumber;
        }

        public string LastFiveIncidentLst(string emailid, string url)
        {
            string incidentNumberLst = string.Empty;
            //string url = _iconfiguration.GetValue<string>("LastFiveIncidentNo"); //"https://hexawaredemo1.service-now.com/api/now/v1/table/incident";
            url = url.Replace("{email}", emailid);
            (bool, string) response = GetData("", url);

            if (response.Item1 && !string.IsNullOrEmpty(response.Item2))
            {
                JObject o = JObject.Parse(response.Item2);
                IList<Object> results = o.SelectToken("result").Select(s => (Object)s).ToList();

                if (results.Count > 0)
                {
                    foreach (Object str in results)
                    {
                        JObject arResult = JObject.Parse(Convert.ToString(str));
                        incidentNumberLst = Convert.ToString(arResult.SelectToken("number")) + " - " + incidentNumberLst;
                    }
                }
            }
            return incidentNumberLst;
        }

        public string CheckIncidentStatusByID(string emailid, string incno, string url)
        {
            string status = string.Empty;
            //string url = _iconfiguration.GetValue<string>("IncidentStatusCheckByID"); //"https://hexawaredemo1.service-now.com/api/now/v1/table/incident";
            url = url.Replace("{email}", emailid).Replace("{incidentId}", incno);
            (bool, string) response = GetData("", url);

            if (response.Item1 && !string.IsNullOrEmpty(response.Item2))
            {
                JObject jObj = JObject.Parse(response.Item2);
                IList<Object> results = jObj.SelectToken("result").Select(s => (Object)s).ToList();

                if (results.Count == 1)
                    status = Convert.ToString(jObj.SelectToken("result[0].state"));
                else
                    status = "Issue";

                //Need check for all other status
                if (status == "12")
                    status = "Work In Progress";
                else if (status == "11")
                    status = "Assigned";
                else if (status == "6")
                    status = "Resolved";
                else if (status == "7")
                    status = "Closed";
                else if (status == "-7")
                    status = "Pending Change";
                else if (status == "-5")
                    status = "Pending User Info";
                else if (status == "-8")
                    status = "Pending Vendor";
            }
            return status;
        }

        public (bool, string) PostData(string json, string customUrl)
        {
            (bool, string) output;
            try
            {
                output = CallApi(json, "POST", string.Empty, customUrl).Result;
            }
            catch (Exception ex)
            {
                output = (false, ex.Message);
            }
            return output;
        }

        public (bool, string) GetData(string json, string customUrl)
        {
            (bool, string) output;
            try
            {
                output = CallApi(json, "GET", string.Empty, customUrl).Result;
            }
            catch (Exception ex)
            {
                output = (false, ex.Message);
            }
            return output;
        }

        internal async Task<(bool, string)> CallApi(string json, string method, string tableName, string customUrl = null)
        {
            // json = _iconfiguration.GetValue<string>("IncidentCreateJson"); //"{\"short_description\":\"Testing\",\"category\":\"General\",\"subcategory\":\"General\"}";
            string response = string.Empty;
            try
            {
                string url = string.Empty;
                if (customUrl != null)
                {
                    url = customUrl;
                }
                else
                {
                    url = url + tableName;
                }
                HttpClientHandler restHandler;
                string proxyAuthNeeded = "No";
                if (proxyAuthNeeded.ToUpper() == "Yes")
                {
                    string proxyUrl = "";
                    string ProxyUserName = "";
                    string ProxyPassword = "";

                    WebProxy proxy = new WebProxy(proxyUrl);
                    restHandler = new HttpClientHandler
                    {
                        Credentials = new NetworkCredential(ProxyUserName, ProxyPassword),
                        Proxy = proxy,
                        UseProxy = true,
                        UseDefaultCredentials = false
                    };
                }
                else
                {
                    restHandler = new HttpClientHandler
                    {
                        UseDefaultCredentials = true
                    };
                }
                restHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                restHandler.SslProtocols = SslProtocols.Tls12;
                restHandler.AllowAutoRedirect = false;

                using (HttpClient rest = new HttpClient(restHandler))
                {
                    rest.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    rest.DefaultRequestHeaders.Add("authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("int_user" + ":" + "Admin@123")));

                    if (method == "GET")
                    {
                        Uri sNowURI = null;
                        if (!string.IsNullOrEmpty(json))
                        {
                            sNowURI = new Uri(url + "?" + json);
                        }
                        else
                        {
                            sNowURI = new Uri(url);
                        }
                        using (HttpResponseMessage res = rest.GetAsync(sNowURI).GetAwaiter().GetResult())
                        {
                            res.EnsureSuccessStatusCode();
                            if (res.IsSuccessStatusCode)
                            {
                                response = res.Content.ReadAsStringAsync().Result;
                            }
                        }
                    }
                    else if (method == "POST")
                    {
                        Uri sNowURI = new Uri(url);
                        using (HttpResponseMessage res = await rest.PostAsync(sNowURI, new StringContent(json, Encoding.UTF8, "application/json")))
                        {
                            res.EnsureSuccessStatusCode();
                            if (res.IsSuccessStatusCode)
                            {
                                response = res.Content.ReadAsStringAsync().Result;
                            }
                        }
                    }
                    else if (method == "XmlPOST")
                    {
                        Uri sNowURI = new Uri(url);
                        using (HttpResponseMessage res = await rest.PostAsync(sNowURI, new StringContent(json, Encoding.UTF8, "application/xml")))
                        {
                            res.EnsureSuccessStatusCode();
                            if (res.IsSuccessStatusCode)
                            {
                                response = res.Content.ReadAsStringAsync().Result;

                            }
                        }
                    }
                    else
                    {
                        Uri sNowURI = new Uri(url);
                        using (HttpResponseMessage res = await rest.PutAsync(sNowURI, new StringContent(json, Encoding.UTF8, "application/json")))
                        {
                            res.EnsureSuccessStatusCode();
                            if (res.IsSuccessStatusCode)
                            {
                                response = res.Content.ReadAsStringAsync().Result;

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string snowDetails = "Snow api url: " + customUrl + ", request: " + json + ", snow response: " + response;
                snowDetails = snowDetails.Replace("\"", "\\\"");
                snowDetails = snowDetails.Replace(",", " ^ nl ^ ");
                return (false, ex.Message + "~" + snowDetails);
            }
            return (true, response);
        }

        public static async System.Threading.Tasks.Task<string> GetToken()
        {
            string response1 = string.Empty;
            string BotId = string.Empty;
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    Dictionary<string, string> tokenDetails = null;

                    HttpClientHandler restHandler = new HttpClientHandler();
                    restHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (HttpClient rest = new HttpClient(restHandler))
                    {
                        rest.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        rest.DefaultRequestHeaders.Add("username", "BotApi");
                        rest.DefaultRequestHeaders.Add("password", "v$5}8g_90m");

                        var url = "http://192.168.225.144:16200/GetToken";
                        var dict = new Dictionary<string, string>();
                        dict.Add("grant_type", "password");

                        using (HttpResponseMessage res = await rest.PostAsync(url, new FormUrlEncodedContent(dict)))
                        {
                            res.EnsureSuccessStatusCode();

                            if (res.IsSuccessStatusCode)
                            {
                                tokenDetails = JsonConvert.DeserializeObject<Dictionary<string, string>>(res.Content.ReadAsStringAsync().Result);
                                if (tokenDetails != null && tokenDetails.Any())
                                {
                                    var tokenNo = tokenDetails.FirstOrDefault().Value;
                                    response1 = tokenNo;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return response1;
        }

        public static async System.Threading.Tasks.Task<string> ValidateUserID(string json)
        {
            var tokenNo = GetToken();
            string response = string.Empty;
            //string json = "{\"UserID\":\"{UserID}\",\"Password\":\"null\",\"Activity\":\"\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"MobileNumber\":\"null\",\"QuestionAnswerModelList\":\"null\"}";
            //json = json.Replace("{UserID}", "51296").Replace("{sessionId}", "testing");
            //Logger.LogError("ValidateEnrollUser : " + json + " token :  " + tokenNo);

            try
            {
                HttpClientHandler restHandler = new HttpClientHandler();
                restHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                HttpClient rest1 = new HttpClient(restHandler);
                var url1 = "http://192.168.225.144:16200/api/user/ValidateUserID";
                string tokenResNo = tokenNo.Result.ToString();
                rest1.DefaultRequestHeaders.Add("Authorization", "bearer " + tokenResNo);

                var content = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                HttpResponseMessage res1 = await rest1.PostAsync(url1, new StringContent(json, Encoding.UTF8, "application/json"));

                if (res1.IsSuccessStatusCode)
                {
                    response = res1.Content.ReadAsStringAsync().Result;
                    //Logger.LogError("ValidateEnrollUser : " + json + " token :  " + tokenNo + " response : "+ response);

                    return response;
                    //if (response.Contains("Invalid Userid"))
                    //    return false;
                    //else
                    //    return true;
                }
            }
            catch (Exception ex)
            {
                return ex.Message.ToString(); ;
            }

            return response;
        }

        public static async System.Threading.Tasks.Task<bool> ValidateEnrollUser(string json)
        {
            var tokenNo = GetToken();
            string response = string.Empty;
            //string json = "{\"UserID\":\"{UserID}\",\"Password\":\"null\",\"Activity\":\"\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"MobileNumber\":\"null\",\"QuestionAnswerModelList\":\"null\"}";
            //json = json.Replace("{UserID}", "51296").Replace("{sessionId}", "testing");
            //Logger.LogError("ValidateEnrollUser : " + json + " token :  " + tokenNo);

            try
            {
                HttpClientHandler restHandler = new HttpClientHandler();
                restHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                HttpClient rest1 = new HttpClient(restHandler);
                var url1 = "http://192.168.225.144:16200/api/user/ValidateUserID";
                string tokenResNo = tokenNo.Result.ToString();
                rest1.DefaultRequestHeaders.Add("Authorization", "bearer " + tokenResNo);

                var content = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                HttpResponseMessage res1 = await rest1.PostAsync(url1, new StringContent(json, Encoding.UTF8, "application/json"));

                if (res1.IsSuccessStatusCode)
                {
                    response = res1.Content.ReadAsStringAsync().Result;
                    //Logger.LogError("ValidateEnrollUser : " + json + " token :  " + tokenNo + " response : "+ response);

                    if (response.Contains("Invalid Userid"))
                        return false;
                    else
                        return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static async System.Threading.Tasks.Task<bool> SendOTP(string json)
        {
            var tokenNo = GetToken();
            string response = string.Empty;
            //string json = "{\"UserID\":\"{UserID}\",\"Password\":\"{Password}\",\"Activity\":\"UserEnrollment\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"MobileNumber\":\"{MobileNumber}\",\"IsPrivate\":\"false\",\"IsRegisteredForOTP\":\"false\",\"CountryCode\":\"{CountryCode}\",\"QuestionAnswerModelList\":\"null\"}";
            //json = json.Replace("{UserID}", "48123").Replace("{Password}", "Google@123").Replace("{sessionId}", "testing").Replace("{MobileNumber}", "9791829901").Replace("{CountryCode}", "91");
            //Logger.LogError("SendOTP : " + json + " token :  " + tokenNo);

            try
            {
                HttpClientHandler restHandler = new HttpClientHandler();
                restHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                HttpClient rest1 = new HttpClient(restHandler);
                var url1 = "http://192.168.225.144:16200/api/user/AuthorizeUserAndSendOTP";
                string tokenResNo = tokenNo.Result.ToString();

                rest1.DefaultRequestHeaders.Add("Authorization", "bearer " + tokenResNo);

                var content = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                HttpResponseMessage res1 = await rest1.PostAsync(url1, new StringContent(json, Encoding.UTF8, "application/json"));

                if (res1.IsSuccessStatusCode)
                {
                    response = res1.Content.ReadAsStringAsync().Result;
                    ///Logger.LogError("SendOTP : " + json + " token :  " + tokenNo + " response : " + response);

                    if (response.Contains("AuthorizationFailed"))
                        return false;
                    else
                        return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static async System.Threading.Tasks.Task<bool> SendOTPForAuthorization(string json)
        {
            var tokenNo = GetToken();
            string response = string.Empty;
            //string json = "{\"UserID\":\"{UserID}\",\"Password\":\"{Password}\",\"Activity\":\"UserEnrollment\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"MobileNumber\":\"{MobileNumber}\",\"IsPrivate\":\"false\",\"IsRegisteredForOTP\":\"false\",\"CountryCode\":\"{CountryCode}\",\"QuestionAnswerModelList\":\"null\"}";
            //json = json.Replace("{UserID}", "48123").Replace("{Password}", "Google@123").Replace("{sessionId}", "testing").Replace("{MobileNumber}", "9791829901").Replace("{CountryCode}", "91");
            //Logger.LogError("SendOTP : " + json + " token :  " + tokenNo);

            try
            {
                HttpClientHandler restHandler = new HttpClientHandler();
                restHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                HttpClient rest1 = new HttpClient(restHandler);
                var url1 = "http://192.168.225.144:16200/api/user/SendOTPForAuthorization";
                string tokenResNo = tokenNo.Result.ToString();

                rest1.DefaultRequestHeaders.Add("Authorization", "bearer " + tokenResNo);

                var content = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                HttpResponseMessage res1 = await rest1.PostAsync(url1, new StringContent(json, Encoding.UTF8, "application/json"));

                if (res1.IsSuccessStatusCode)
                {
                    response = res1.Content.ReadAsStringAsync().Result;
                    ///Logger.LogError("SendOTP : " + json + " token :  " + tokenNo + " response : " + response);

                    if (response.Contains("AuthorizationFailed"))
                        return false;
                    else
                        return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static async System.Threading.Tasks.Task<bool> ValidateOTP(string json)
        {
            var tokenNo = GetToken();
            string response = string.Empty;
            //string json = "{\"UserID\":\"{UserID}\",\"Password\":\"{Password}\",\"Activity\":\"UserEnrollment\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"MobileNumber\":\"{MobileNumber}\",\"IsPrivate\":\"false\",\"IsRegisteredForOTP\":\"false\",\"CountryCode\":\"{CountryCode}\",\"OTP\":\"{OTP}\",\"QuestionAnswerModelList\":\"null\"}";
            //json = json.Replace("{UserID}", "48123").Replace("{Password}", "Google@123").Replace("{sessionId}", "testing").Replace("{MobileNumber}", "9791829901").Replace("{CountryCode}", "91").Replace("{OTP}", "852753");
            //Logger.LogError("ValidateOTP : " + json + " token :  " + tokenNo);

            try
            {
                HttpClientHandler restHandler = new HttpClientHandler();
                restHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                HttpClient rest1 = new HttpClient(restHandler);
                var url1 = "http://192.168.225.144:16200/api/user/ValidateOTPForRegistration";

                string tokenResNo = tokenNo.Result.ToString();

                rest1.DefaultRequestHeaders.Add("Authorization", "bearer " + tokenResNo);

                var content = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                HttpResponseMessage res1 = await rest1.PostAsync(url1, new StringContent(json, Encoding.UTF8, "application/json"));

                if (res1.IsSuccessStatusCode)
                {
                    response = res1.Content.ReadAsStringAsync().Result;
                    //Logger.LogError("ValidateOTP : " + json + " token :  " + tokenNo + " response : " + response);

                    if (response.Contains("false"))
                        return false;
                    else
                        return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static async System.Threading.Tasks.Task<Root> GetAllSecurityQuestions(string json)
        {
            Root secQns = new Root();
            var tokenNo = GetToken();
            string response = string.Empty;
            //string json = "{\"UserID\":\"{UserID}\",\"Password\":\"null\",\"NewPassword\":\"null\",\"ConfirmNewPassword\":\"null\",\"sessionId\":\"{sessionId}\",\"sourceorigin\":\"1\",\"OTP\":\"null\",\"QuestionAnswerModelList\":\"null\"}";
            //json = json.Replace("{UserID}", "48123").Replace("{sessionId}", "qwqwqqwqw");
            //Logger.LogError("ValidateOTP : " + json + " token :  " + tokenNo);
            Dictionary<string, string> tokenDetails = null;
            try
            {
                HttpClientHandler restHandler = new HttpClientHandler();
                restHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                HttpClient rest1 = new HttpClient(restHandler);
                var url1 = "http://192.168.225.144:16200/api/user/GetAllSecurityQuestions";

                string tokenResNo = tokenNo.Result.ToString();

                rest1.DefaultRequestHeaders.Add("Authorization", "bearer " + tokenResNo);

                var content = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                HttpResponseMessage res1 = await rest1.PostAsync(url1, new StringContent(json, Encoding.UTF8, "application/json"));

                if (res1.IsSuccessStatusCode)
                {
                    //response = res1.Content.ReadAsStringAsync().Result;
                    //Logger.LogError("ValidateOTP : " + json + " token :  " + tokenNo + " response : " + response);

                    secQns = JsonConvert.DeserializeObject<Root>(res1.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                return secQns;
            }

            return secQns;
        }

        public static async System.Threading.Tasks.Task<CaptchaResult> GetCaptcha()
        {
            CaptchaResult captchaResult = new CaptchaResult();
            try
            {
                HttpClientHandler restHandler = new HttpClientHandler();
                restHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                HttpClient rest1 = new HttpClient(restHandler);
                var url1 = "http://localhost:3978/api/get-captcha-image";

                //rest1.DefaultRequestHeaders.Add("Authorization", "bearer " + tokenResNo);

                HttpResponseMessage res1 = await rest1.PostAsync(url1, new StringContent("", Encoding.UTF8, "application/json"));

                if (res1.IsSuccessStatusCode)
                {
                    //response = res1.Content.ReadAsStringAsync().Result;
                    //Logger.LogError("ValidateOTP : " + json + " token :  " + tokenNo + " response : " + response);

                    captchaResult = JsonConvert.DeserializeObject<CaptchaResult>(res1.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                return captchaResult;
            }

            return captchaResult;
        }

    }
}
