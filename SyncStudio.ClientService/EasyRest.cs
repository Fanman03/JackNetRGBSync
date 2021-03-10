using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SyncStudio.ClientService
{
    public class EasyRest
    {
        private readonly string _baseUrl;

        public EasyRest(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        private static readonly HttpClient client = new HttpClient();
        
        public async Task<TReturnType> Post<TReturnType, TModelType>(string url, TModelType model)
        {
     
                var response = await client.PostAsync(_baseUrl + url,
                    new StringContent(JsonConvert.SerializeObject(model)));

                var responseString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<TReturnType>(responseString);
    
        }

        public async Task<TReturnType> Get<TReturnType>(string url)
        {
            var response = await client.GetAsync(_baseUrl + url);

                var responseString = await response.Content.ReadAsStringAsync();

                TReturnType result = default;
                try
                {
                    result = JsonConvert.DeserializeObject<TReturnType>(responseString);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                return result;
        }

        public TReturnType GetSync<TReturnType>(string url)
        {
            using (var webClient = new WebClient())
            {
                string responseString = webClient.DownloadString(_baseUrl + url);
                TReturnType result = default;
                try
                {
                    result = JsonConvert.DeserializeObject<TReturnType>(responseString);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                return result;
            }
        }

        public TReturnType PostSync<TReturnType, TModelType>(string url, TModelType model)
        {
            using (var webClient = new WebClient())
            {
                string json = JsonConvert.SerializeObject(model);
                var responseString = webClient.UploadString(_baseUrl + url, json);
                return JsonConvert.DeserializeObject<TReturnType>(responseString);
            }
        }

        public void PostSync<TModelType>(string url, TModelType model)
        {

            //var request = (HttpWebRequest)WebRequest.Create(_baseUrl + url);
            //string json = JsonConvert.SerializeObject(model);
            //var data = Encoding.ASCII.GetBytes(json);

            //request.Method = "POST";
            //request.ContentType = "application/json";
            //request.ContentLength = data.Length;

            //using (var stream = request.GetRequestStream())
            //{
            //    stream.Write(data, 0, data.Length);
            //}

            //var response = (HttpWebResponse)request.GetResponse();

            //var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            using (var webClient = new WebClient())
            {
                string json = JsonConvert.SerializeObject(model);
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                var responseString = webClient.UploadString(_baseUrl + url, json);
            }
        }
    }
}
