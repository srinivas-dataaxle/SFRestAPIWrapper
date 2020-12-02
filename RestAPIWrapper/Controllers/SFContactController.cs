using Newtonsoft.Json;
using RestAPIWrapper.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

namespace RestAPIWrapper.Controllers
{
    public class SFContactController : ApiController
    {

        private static readonly log4net.ILog log =
    log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            log.Info("In Get()...");
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            log.Info("In Get...with id: " + id);
            return "value";
        }

        // POST api/<controller>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public string Post(Newtonsoft.Json.Linq.JObject contact)
        {

            var re = Request;
            var headers = re.Headers;
            //string token = "";
           
            try
            {

                var reqHeaders = re.Headers;

               

                string signatureVersion = "";

                if (reqHeaders.Contains("X-HubSpot-Signature-Version"))
                {
                    signatureVersion = reqHeaders.GetValues("X-HubSpot-Signature-Version").First();
                }

                log.Info("signatureVersion:" + signatureVersion);

                string authHash = "";

                if (reqHeaders.Contains("X-HubSpot-Signature"))
                {
                    authHash = reqHeaders.GetValues("X-HubSpot-Signature").First();
                }

                log.Info("Auth Hash:" + authHash);

                string hubSpotSecret = ConfigurationManager.AppSettings["HubSpotClientSecret"];

                string reqJSON = new StringContent(JsonConvert.SerializeObject(contact)).ReadAsStringAsync().Result;

                log.Info("Request JSON:" + reqJSON);

                string verificationHash = hubSpotSecret + "POST" + ConfigurationManager.AppSettings["API_URL"] + reqJSON;

                log.Info("Verification Hash:" + ComputeSha256Hash(verificationHash));

                log.Info("Req JSON:" + reqJSON);


                //if (headers.Contains("Authorization"))
                //{
                //    token = headers.GetValues("Authorization").First();
                //}

                //string hubSpotHeaders = String.Empty;
                //foreach (var header in reqHeaders)
                //    hubSpotHeaders += header.Key + "=" + header.Value + Environment.NewLine;

                //log.Info("Req Headers:" + hubSpotHeaders);

                //IEnumerable<KeyValuePair<String, String>> reqParams = re.GetQueryNameValuePairs();

                //foreach (KeyValuePair<String, String> reqParam in reqParams)
                //{
                //    if ("CampaignName".Equals(reqParam.Key, StringComparison.OrdinalIgnoreCase))
                //    {
                //        campaignName = reqParam.Value;
                //        break;
                //    }
                //}

                SFClient sfClient = new SFClient
                {
                    Username = ConfigurationManager.AppSettings["username"],
                    Password = ConfigurationManager.AppSettings["password"],
                    Token = ConfigurationManager.AppSettings["token"],
                    ClientId = ConfigurationManager.AppSettings["clientId"],
                    ClientSecret = ConfigurationManager.AppSettings["clientSecret"]
                };

                sfClient.Login();
                sfClient.UpdateContact(contact);

                return "Success";
            } catch (Exception e)
            {
                log.Error("Exception: ", e);
                return "Failure:" + e.StackTrace;
            }

        }


        static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

    }
}