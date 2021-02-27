using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace FunctionApp1
{
    public static class TimerTriggerFunction
    {
        [FunctionName("TimerTriggerForWebhook")]
        public static void Run([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            #region get data from source e.g: reqres.in 
            string responseString = "";
            var rnd = new Random();
            int id = rnd.Next(1, 10);
            string baseURI = "https://reqres.in/";
            string requestParam = "api/users/" + id;

            string responseBody = doHttpRequest("GET", baseURI, requestParam);
            log.LogInformation("Get Response: " + responseBody + "\n\n");
            JObject json = JObject.Parse(responseBody);
            string data = json["data"].ToString();
            #endregion

            #region post data to webhookinbox 
            baseURI = "http://api.webhookinbox.com/";
            requestParam = "i/vXWYRz8A/in/";
            responseString = doHttpRequest("POST", baseURI, requestParam, data);
            log.LogInformation("Post Response: " + responseString + "\n\n");
            #endregion
        }



        public static string doHttpRequest(string httpMethod, string baseURI, string requestParam, string data = "")
        {
            string responseString = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseURI);
                client.DefaultRequestHeaders.Accept.Clear();
                HttpRequestMessage request = null;
                if (httpMethod == "GET")
                {
                    request = new HttpRequestMessage(HttpMethod.Get, requestParam);
                }
                if (httpMethod == "POST")
                {
                    request = new HttpRequestMessage(HttpMethod.Post, requestParam);
                    request.Content = new StringContent(data, Encoding.UTF8, "application/json");
                }
                var response = client.SendAsync(request).GetAwaiter().GetResult();
                responseString = response.Content.ReadAsStringAsync().Result.ToString();
            }
            return responseString;
        }
    }
}