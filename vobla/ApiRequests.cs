﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.IO;

namespace vobla
{
    public class ApiRequests
    {
        private static readonly ApiRequests instance = new ApiRequests();

        public static ApiRequests Instance
        {
            get
            {
                return instance;
            }
        }

        private class LoginData
        {
            public string email { get; set; }
            public string password { get; set; }
        }

        private System.Net.Http.HttpClient httpclient = null;

        private ApiRequests()
        {
            this.httpclient = new HttpClient();
            this.httpclient.BaseAddress = new Uri(new Uri(vobla.Properties.Settings.Default.URL), "/api/");
        }

        public void SetToken(string token)
        {

            this.httpclient.DefaultRequestHeaders.Remove("Auth-token");
            this.httpclient.DefaultRequestHeaders.Add("Auth-token", token);
        }

        public async void LoginPost(string email, string password)
        {
            LoginData data = new LoginData { email = email, password = password };
            var response = await this.httpclient.PostAsJsonAsync("user/login", data);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                JObject jData = JObject.Parse(result);
                string token = (string)jData["token"];
                vobla.Properties.Settings.Default.Token = token;
                this.SetToken(token);
                Console.WriteLine(vobla.Properties.Settings.Default.Token);
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, result);
            }
        }

        private byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }

        public async Task<String> ImagePost(Image img, string imageName = "screenshot name")
        {
            var formData = new MultipartFormDataContent();
            HttpContent bytesContent = new ByteArrayContent(this.ImageToByteArray(img));
            formData.Add(bytesContent, "file", imageName);
            var response = await this.httpclient.PostAsync("files/upload", formData);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                JObject jData = JObject.Parse(result);
                string url = (string)(jData["file"]["url"]);
                Console.WriteLine(url);
                return url;
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, result);
                return null;
            }
        }
    }
}