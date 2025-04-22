using BExIS.Dim.Entities.Export.GBIF;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dim.Helpers.GBIF
{
    public  class GbifServiceManager
    {
        private GBFICrendentials _crendentials;
   
        public GbifServiceManager(GBFICrendentials crendentials)
        {
            _crendentials = crendentials;
        }

        public async Task<HttpResponseMessage> CreateDataset(GbifDataType type, string title)
        {
            try
            {
                string requestUrl = _crendentials.Server + "/dataset";
                GbifCreateDatasetRequest request = new GbifCreateDatasetRequest()
                {
                    InstallationKey = _crendentials.InstallationKey,
                    PublishingOrganizationKey = _crendentials.OrganisationKey,
                    Type = type,
                    Title = title
                };

                string json = JsonConvert.SerializeObject(request);

                using (var client = new HttpClient())
                {
                    string requesturl = _crendentials.Server + "/dataset";

                    using (HttpContent content = new StringContent(json, Encoding.UTF8, "application/json"))
                    {

                        client.BaseAddress = new Uri(requesturl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        //test@testerer.de:WSTest
                        var byteArray = Encoding.ASCII.GetBytes(_crendentials.Username + ":" + _crendentials.Password);

                        // "basic "+ Convert.ToBase64String(byteArray)
                        AuthenticationHeaderValue ahv = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                        client.DefaultRequestHeaders.Authorization = ahv;

                        //HttpResponseMessage response = await client.PostAsync(requesturl, sContent);
                        HttpResponseMessage response = await client.PostAsync(requesturl, content);
                        response.EnsureSuccessStatusCode();
                        return ((HttpResponseMessage)response);
                    }
                }

            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        // Get Dataset
        public async Task<HttpResponseMessage> GetDataset(string key)
        {
            try
            {
                string requestUrl = _crendentials.Server + "/dataset";


 
                using (var client = new HttpClient())
                {
                    string requesturl = _crendentials.Server + "/dataset/"+key;


                        client.BaseAddress = new Uri(requesturl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        //test@testerer.de:WSTest
                        var byteArray = Encoding.ASCII.GetBytes(_crendentials.Username + ":" + _crendentials.Password);

                        // "basic "+ Convert.ToBase64String(byteArray)
                        AuthenticationHeaderValue ahv = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                        client.DefaultRequestHeaders.Authorization = ahv;

                        //HttpResponseMessage response = await client.PostAsync(requesturl, sContent);
                        HttpResponseMessage response = await client.GetAsync(requesturl);
                        response.EnsureSuccessStatusCode();
                        return ((HttpResponseMessage)response);
                }

            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        // Update Dataset

        // Delete Dataset
        public async Task<HttpResponseMessage> DeleteDataset(string key)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string requesturl = _crendentials.Server + "/dataset/"+key;
 
                    client.BaseAddress = new Uri(requesturl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    //test@testerer.de:WSTest
                    var byteArray = Encoding.ASCII.GetBytes(_crendentials.Username + ":" + _crendentials.Password);

                    // "basic "+ Convert.ToBase64String(byteArray)
                    AuthenticationHeaderValue ahv = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    client.DefaultRequestHeaders.Authorization = ahv;

                    //HttpResponseMessage response = await client.PostAsync(requesturl, sContent);
                    HttpResponseMessage response = await client.DeleteAsync(requesturl);
                    response.EnsureSuccessStatusCode();
                    return ((HttpResponseMessage)response);
                    
                }

            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        public async Task<HttpResponseMessage> AddEndpoint(string datasetKey, string url)
        {
            try
            {
                // clean datasetKey
                datasetKey = datasetKey.Replace('\"', ' ');
                datasetKey = datasetKey.Trim();

                GbifAddEndpointRequest request = new GbifAddEndpointRequest(url);
             
                string json = JsonConvert.SerializeObject(request);

                using (var client = new HttpClient())
                {
                    string requesturl = _crendentials.Server + "/dataset/" + datasetKey + "/endpoint";

                    using (HttpContent content = new StringContent(json, Encoding.UTF8, "application/json"))
                    {

                        client.BaseAddress = new Uri(requesturl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        //test@testerer.de:WSTest
                        var byteArray = Encoding.ASCII.GetBytes(_crendentials.Username + ":" + _crendentials.Password);

                        // "basic "+ Convert.ToBase64String(byteArray)
                        AuthenticationHeaderValue ahv = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                        client.DefaultRequestHeaders.Authorization = ahv;

                        //HttpResponseMessage response = await client.PostAsync(requesturl, sContent);
                        HttpResponseMessage response = await client.PostAsync(requesturl, content);
                        response.EnsureSuccessStatusCode();
                        return ((HttpResponseMessage)response);
                    }
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        // delete endpoint



    }
}
