using Murky.Utils;
using Murky.Utils.CSGO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CSAuto
{
    public static class ImageLoader
    {
        static readonly string IMAGES_PATH = Log.WorkPath + "\\resource\\images.pac";
        static readonly string fileData = App.Unzip(File.ReadAllBytes(IMAGES_PATH));
        static readonly Dictionary<string,BitmapImage> cachedImages = new Dictionary<string,BitmapImage>();
        public static BitmapImage Base64StringToBitmap(string base64String)
        {
            byte[] byteBuffer = Convert.FromBase64String(base64String);
            MemoryStream memoryStream = new MemoryStream(byteBuffer);
            memoryStream.Position = 0;

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }
        public static BitmapImage LoadBitmapImage(string image)
        {
            string actualImageName = $"[{image.ToUpper()}]";
            if (cachedImages.ContainsKey(actualImageName))
                return cachedImages[actualImageName];
            if (!fileData.Contains(actualImageName))
                return null;
            string imageData = fileData.Split(new string[] { actualImageName }, StringSplitOptions.None)[1].
                Split(new string[] { "[END]" }, StringSplitOptions.None)[0];

            BitmapImage res = Base64StringToBitmap(imageData);
            cachedImages[actualImageName] = res;
            return res;
        }
    }
}
