using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace Client.Utilities
{
    public class APIHelper : IAPIHelper
    {
        public HttpResponseMessage GetResponse(string url, string token)
        {
            using (var client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);
                }
                var response = client.GetAsync(url).Result;
                if (!response.IsSuccessStatusCode)
                {
                    var message = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result).ToString();
                    throw new HttpException((int)response.StatusCode, message);
                }
                return response;
            }
        }

        public HttpResponseMessage PostRequest(string url, string token)
        {
            using (var client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);
                }
                var response = client.PostAsync(url, new FormUrlEncodedContent(new List<KeyValuePair<string, string>>())).Result;
                if (!response.IsSuccessStatusCode)
                {
                    var message = response.Content.ReadAsAsync<WebExceptionObject>().Result.ExceptionMessage;
                    throw new HttpException((int)response.StatusCode, message);
                }
                return response;
            }
        }
    }
}