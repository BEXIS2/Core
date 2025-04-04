using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dim.Helpers
{
    public class BasicWebService
    {
        public static async Task<string> Call(string url, string user, string password, string parameters = "")
        {
            string returnValue = "";

            try
            {
                using (var client = new HttpClient())
                {
                    //generate url

                    Debug.WriteLine(url);

                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    //test@testerer.de:WSTest
                    var byteArray = Encoding.ASCII.GetBytes(user + ":" + password);

                    // "basic "+ Convert.ToBase64String(byteArray)
                    AuthenticationHeaderValue ahv = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    client.DefaultRequestHeaders.Authorization = ahv;

                    string requesturl = url + parameters;
                    HttpResponseMessage response = await client.GetAsync(requesturl);
                    Debug.WriteLine(requesturl);
                    //Debug.WriteLine(Server.UrlEncode(parameters));
                    response.EnsureSuccessStatusCode();
                    returnValue = ((HttpResponseMessage)response).Content.ReadAsStringAsync().Result;
                }
                return returnValue;
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        public static async Task<string> Call(string url, string user, string password, string parameters = "", string body = "")
        {
            string returnValue = "";

            try
            {
                using (var client = new HttpClient())
                {
                    //generate url

                    Debug.WriteLine(url);

                    string requesturl = url + parameters;

                    using (HttpContent content = new StringContent(body, Encoding.UTF8, "application/json"))
                    {

                        client.BaseAddress = new Uri(url);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        //test@testerer.de:WSTest
                        var byteArray = Encoding.ASCII.GetBytes(user + ":" + password);

                        // "basic "+ Convert.ToBase64String(byteArray)
                        AuthenticationHeaderValue ahv = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                        client.DefaultRequestHeaders.Authorization = ahv;

                        //HttpResponseMessage response = await client.PostAsync(requesturl, sContent);
                        HttpResponseMessage response = await client.PostAsync(requesturl, content);
                        Debug.WriteLine(requesturl);
                        //Debug.WriteLine(Server.UrlEncode(parameters));
                        response.EnsureSuccessStatusCode();
                        return ((HttpResponseMessage)response).Content.ReadAsStringAsync().Result;
                    }
                    
                }
                return returnValue;
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
    }
}