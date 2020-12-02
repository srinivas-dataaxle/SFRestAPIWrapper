using Newtonsoft.Json;
using RestAPIWrapper.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace RestAPIWrapper
{
    public class SFClient
    {

        private static readonly log4net.ILog log =
  log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string LOGIN_ENDPOINT = ConfigurationManager.AppSettings["SFLoginURL"];
        private const string API_ENDPOINT = "/services/apexrest/ContactLead/updateContact";

        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthToken { get; set; }
        public string InstanceUrl { get; set; }

        static SFClient()
        {
            // SF requires TLS 1.1 or 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
        }

        // TODO: use RestSharps
        public void Login()
        {

            log.Info("In Login....");
            String jsonResponse;

            using (var client = new HttpClient())
            {
                var request = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        {"grant_type", "password"},
                        {"client_id", ConfigurationManager.AppSettings["client_id"]},
                        {"client_secret", ConfigurationManager.AppSettings["client_secret"]},
                        {"username", ConfigurationManager.AppSettings["username"]},
                        {"password", ConfigurationManager.AppSettings["password"]}
                    }
                );

                var response = client.PostAsync(LOGIN_ENDPOINT, request).Result;
                jsonResponse = response.Content.ReadAsStringAsync().Result;
            }
            log.Info($"Response: {jsonResponse}");
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
            AuthToken = values["access_token"];
            InstanceUrl = values["instance_url"];
        }


        public String UpdateContact(Newtonsoft.Json.Linq.JObject jcontact)
        {
            log.Info("In UpdateContact....");
            var httpClient = new HttpClient();
           
            dynamic payload = jcontact;
            dynamic contact = null;

            if (payload != null && payload.properties != null)
            {
                contact = payload.properties;
            }

            String email = "";
            String campaignId = "";
            String company = "";
            String firstname = "";
            String lastname = "";

            if (contact != null)
            {

                if (contact.email != null)
                {
                    email = contact.email.value;
                }

                if (contact.sfcampaignid != null)
                {
                    campaignId = contact.sfcampaignid.value;
                }

                if (contact.company != null)
                {
                    company = contact.company.value;
                }

                if (contact.firstname != null)
                {
                    firstname = contact.firstname.value;
                }

                if (contact.lastname != null)
                {
                    lastname = contact.lastname.value;
                }


            }

            log.Info("Email:" + email);
            log.Info("Campaign Id:" + campaignId);
            log.Info("Company:" + company);
            log.Info("First Name:" + firstname);
            log.Info("Last Name:" + lastname);

            if (email == null || campaignId == null)
            {
                return "Failure";
            }

            string url = InstanceUrl + API_ENDPOINT + "?campaignId=" + campaignId +"&company=" + company + "&firstname=" + firstname + "&lastname=" + lastname;

            log.Info("URL:" + url);

            Contact contactObj = new Contact();

            contactObj.Email = email;

            using (var content = new StringContent(JsonConvert.SerializeObject(contactObj), System.Text.Encoding.UTF8, "application/json"))
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + AuthToken);
                HttpResponseMessage result = httpClient.PostAsync(url, content).Result;
                var returnValue = result.Content.ReadAsStringAsync().Result;
            }
            return "Success";
        }

    }
}