using Newtonsoft.Json.Linq;
using System;
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
        public static string EscapeMarkdown(string text)
        {
            return text
                    .Replace("\\", "\\\\")
                    .Replace("_", "\\_")
                    .Replace("*", "\\*")
                    .Replace("[", "\\[")
                    .Replace("]", "\\]")
                    .Replace("(", "\\(")
                    .Replace(")", "\\)")
                    .Replace("~", "\\~")
                    .Replace("`", "\\`")
                    .Replace(">", "\\>")
                    .Replace("#", "\\#")
                    .Replace("+", "\\+")
                    .Replace("-", "\\-")
                    .Replace("=", "\\=")
                    .Replace("|", "\\|")
                    .Replace("{", "\\{")
                    .Replace("}", "\\}")
                    .Replace(".", "\\.")
                    .Replace("!", "\\!");
        }
        public static bool SendMessage(string text,string chatID,string token, bool markdown = false, bool log = true)
        {
            try
            {
                string escapedText = Uri.EscapeDataString(text);

                string urlString = $"https://api.telegram.org/bot{token}/sendMessage?chat_id={chatID}" + (markdown ? "&parse_mode=markdownv2" : "") + $"&text={escapedText}";

                using (WebClient webclient = new WebClient())
                {
                    webclient.DownloadString(urlString);
                }
                if(log)
                    Log.WriteLine($"|Telegram.cs| Sent telegram message \"{text}\"");
                return true;
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
                return false;
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
        public static bool SendTextAsFile(string textContent, string fileName, string chatID, string token, string caption = null)
        {
            try
            {
                string url = $"https://api.telegram.org/bot{token}/sendDocument";

                var bytes = Encoding.UTF8.GetBytes(textContent);
                using (var stream = new MemoryStream(bytes))
                using (var form = new MultipartFormDataContent())
                {
                    form.Add(new StringContent(chatID, Encoding.UTF8), "chat_id");

                    if (caption != null)
                        form.Add(new StringContent(caption, Encoding.UTF8), "caption");

                    form.Add(new StreamContent(stream), "document", fileName);

                    using (HttpClient client = new HttpClient())
                    {
                        var response = client.PostAsync(url, form).GetAwaiter().GetResult();
                        Log.WriteLine($"|Telegram.cs| Sent text file \"{fileName}\" to \"{chatID}\"");
                        return response.IsSuccessStatusCode;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static async void SendFile(Stream fileStream, string fileName, string chatID, string token, string caption = null)
        {
            try
            {
                string url = $"https://api.telegram.org/bot{token}/sendDocument";

                using (var form = new MultipartFormDataContent())
                {
                    form.Add(new StringContent(chatID, Encoding.UTF8), "chat_id");

                    if (caption != null)
                        form.Add(new StringContent(caption, Encoding.UTF8), "caption");

                    form.Add(new StreamContent(fileStream), "document", fileName);

                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.PostAsync(url, form);
                        Log.WriteLine($"|Telegram.cs| Sent telegram file \"{fileName}\" to \"{chatID}\"");
                    }
                }
            }
            catch (WebException ex)
            {
                if (!ex.Message.Contains("(429)") &&
                    ex.Message != "Unable to connect to the remote server")
                {
                    Log.WriteLine($"|Telegram.cs| Error sending file: {ex.Message}");
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
