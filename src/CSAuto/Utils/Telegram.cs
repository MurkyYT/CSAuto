using CSAuto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows;

namespace Murky.Utils
{
    public static class Telegram
    {
        public static void SendMessage(string text,string chatID,string token)
        {
            try
            {
                string urlString = $"https://api.telegram.org/bot{token}/sendMessage?chat_id={chatID}&text={text}";

                using (WebClient webclient = new WebClient())
                {
                    webclient.DownloadString(urlString);
                }
                Log.WriteLine($"|Telegram.cs| Sent telegram message \"{text}\" to \"{chatID}\"");
            }
            catch (WebException ex)
            {
                if (!ex.Message.Contains("(429)") &&
                    ex.Message != "Unable to connect to the remote server")
                {
                    MessageBox.Show($"{CSAuto.Languages.Strings.ResourceManager.GetString("error_telegrammessage")}\n'{ex.Message}'\n'{text}'",
                        CSAuto.Languages.Strings.ResourceManager.GetString("title_error"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
        public static async void SendPhoto(Image photo, string chatID, string token,string caption=null)
        {
            try
            {
                string url = $"https://api.telegram.org/bot{token}/sendPhoto";
                using (var form = new MultipartFormDataContent())
                {
                    form.Add(new StringContent(chatID, Encoding.UTF8), "chat_id");
                    if(caption != null)
                        form.Add(new StringContent(caption, Encoding.UTF8), "caption");
                    using (MemoryStream stream = LoadImageStream(photo))
                    {
                        form.Add(new StreamContent(stream), "photo", "photo.png");
                        using (HttpClient client = new HttpClient())
                        {
                            var response = await client.PostAsync(url, form);
                            Log.WriteLine($"|Telegram.cs| Sent telegram photo to \"{chatID}\"");
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (!ex.Message.Contains("(429)") &&
                    ex.Message != "Unable to connect to the remote server")
                {
                   
                }
            }
        }

        public static bool CheckToken(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token)) return false;
                string url = $"https://api.telegram.org/bot{token}/getMe";
                using (WebClient webclient = new WebClient())
                {
                    string res = webclient.DownloadString(url);
                    JObject jo = JObject.Parse(res);
                    bool isOk = (bool)jo["ok"];
                    return isOk;
                }
            }
            catch
            {
                return false;
            }
        }

        private static MemoryStream LoadImageStream(Image photo)
        {
            ImageConverter _imageConverter = new ImageConverter();
            byte[] paramFileStream = (byte[])_imageConverter.ConvertTo(photo, typeof(byte[]));
            return new MemoryStream(paramFileStream);
        }
    }
}
