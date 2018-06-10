using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.IO;

namespace vobla
{
    public class ApiRequests
    {
        private static readonly ApiRequests instance = new ApiRequests();

        public static ApiRequests Instance => instance;

        private class LoginData
        {
            public string email { get; set; }
            public string password { get; set; }
        }

        private readonly System.Net.Http.HttpClient _httpClient = null;

        private ApiRequests()
        {
            this._httpClient = new HttpClient { };
            this._httpClient.BaseAddress = new Uri(new Uri(vobla.Properties.Settings.Default.URL), "/api/");
        }

        public void SetToken(string token)
        {
            this._httpClient.DefaultRequestHeaders.Remove("Authorization");
            this._httpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");
        }

        public async Task<bool> SyncGet()
        {
            var response = await this._httpClient.GetAsync("users/jwtcheck");
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                Console.WriteLine($"{response.StatusCode} ({result})");
                return false;
            }
        }

        public async Task<Dictionary<String, String>> LoginPost(string email, string password)
        {
            LoginData data = new LoginData { email = email, password = password };
            var response = await this._httpClient.PostAsJsonAsync("users/login", data);
            var result = await response.Content.ReadAsStringAsync();
            var dict = new Dictionary<String, String>();
            if (response.IsSuccessStatusCode)
            {
                JObject jData = JObject.Parse(result);
                foreach (var node in jData)
                {
                    dict[node.Key] = (string)node.Value;
                }
            }
            else
            {
                Console.WriteLine($"{response.StatusCode} ({result})");
            }
            return dict;
        }

        private byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        public async Task<HttpResponseMessage> UploadChunk(byte[] imageBytes,
                                                           string chunkNumber,
                                                           string chunkSize,
                                                           string fileTotalSize,
                                                           string dropHash=null,
                                                           string dropFileHash=null)
        {
            var formData = new MultipartFormDataContent();
            ByteArrayContent bytesContent = new ByteArrayContent(imageBytes);
            if (dropHash != null)
            {
                formData.Headers.Add("Drop-Hash", dropHash);
            }
            if (dropFileHash != null)
            {
                formData.Headers.Add("Drop-File-Hash", dropFileHash);
            }
            formData.Headers.Add("Chunk-Number", chunkNumber);
            formData.Headers.Add("Chunk-Size", chunkSize);
            formData.Headers.Add("File-Total-Size", fileTotalSize);
            formData.Add(bytesContent, "chunk", "file");
            Console.WriteLine(formData.Headers.ToString());
            return await this._httpClient.PostAsync("drops/upload/chunks", formData);
        }

        public async Task<String> ImagePost(Image img, string imageName = "screenshotname.png")
        {
            var CHUNK_SIZE = 100000;
            var chunkNumber = 1;
            var imageBytes = this.ImageToByteArray(img);
            Func<int> calculateLen = () => Math.Min(imageBytes.Length - ((chunkNumber - 1) * CHUNK_SIZE), CHUNK_SIZE);
            var length = calculateLen();
            byte[] buffer = new byte[length];
            Buffer.BlockCopy(imageBytes, Math.Min((chunkNumber - 1) * CHUNK_SIZE, imageBytes.Length), buffer, 0, length);
            var response = await this.UploadChunk(buffer,
                                              chunkNumber.ToString(),
                                              CHUNK_SIZE.ToString(),
                                              imageBytes.Length.ToString(),
                                              null,
                                              null);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                JObject jData = JObject.Parse(result);
                var dropFileHash = (string)jData["drop_file_hash"];
                Console.WriteLine(dropFileHash);
                chunkNumber++;
                length = calculateLen();
                while (length > 0)
                {
                    buffer = null;
                    buffer = new byte[length];
                    Buffer.BlockCopy(imageBytes, (chunkNumber - 1) * CHUNK_SIZE, buffer, 0, length);
                    response = await this.UploadChunk(buffer,
                                              chunkNumber.ToString(),
                                              CHUNK_SIZE.ToString(),
                                              imageBytes.Length.ToString(),
                                              null,
                                              dropFileHash);
                    chunkNumber++;
                    length = calculateLen();
                }
                return dropFileHash;
            }
            else
            {
                Console.WriteLine($"{response.StatusCode} ({result})");
                return null;
            }
        }
    }
}
