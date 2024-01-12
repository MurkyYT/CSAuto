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
        static string IMAGES_PATH = Log.WorkPath + "\\resource\\images.pac";
        static string fileData = App.Unzip(File.ReadAllBytes(IMAGES_PATH));
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

            memoryStream = null;
            byteBuffer = null;

            return bitmapImage;
        }
        public static BitmapImage LoadBitmapImage(string image)
        {

            string actualImageName = $"[{image.ToUpper()}]";
            if (!fileData.Contains(actualImageName))
                return null;
            string imageData = fileData.Split(new string[] { actualImageName }, StringSplitOptions.None)[1].
                Split(new string[] { "[END]" }, StringSplitOptions.None)[0];

            return Base64StringToBitmap(imageData);
        }
    }
}
