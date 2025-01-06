using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using PMS.Shared.Models;

namespace PMS.Shared.Services
{
    public class RestClient
    {
        string _restAPI = string.Empty;
        string _token = string.Empty;
        public RestClient(string restApi)
        {
            _restAPI = restApi;
        }
        public RestClient(string restApi, string token)
        {
            _restAPI = restApi;
            _token = token;
        }

        public async Task<string> GetStringAsync()
        {
            using (HttpClient client = new HttpClient())
            {


                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrWhiteSpace(_token))
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + _token);


                var response = await client.GetAsync(_restAPI);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new ExceptionInvalidToken();
                var errorMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());
                throw new Exception(response.StatusCode.ToString() + " : " + errorMessage);

            }
        }

        public async Task<Stream> GetStreamAsync()
        {
            using (HttpClient client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrWhiteSpace(_token))
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + _token);
                var response = await client.GetAsync(_restAPI);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStreamAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new ExceptionInvalidToken();
                var errorMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());
                throw new Exception(response.StatusCode.ToString() + " : " + errorMessage);
            }
        }
    }
}
