using Murky.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace CSAuto
{
    class BuyItem
    {
        private bool isEnabled;
        private string name;
        private Point position;
        private Size size;
        private bool isGrenade;
        public BuyItem(string name, Point position, Size size, bool isGrenade)
        {
            this.name = name;
            this.position = position;
            this.size = size;
            isEnabled = false;
            this.isGrenade = isGrenade;
        }
        public void SetEnabled(bool isEnabled) { this.isEnabled = isEnabled; }
        public bool IsEnabled() { return isEnabled; }
        public bool IsGrenade() { return isGrenade; }
        public string Name { get { return name; } }
        public Point Position { get { return position; } }
        public Size Size { get { return size; } }
    }
    class AutoBuyMenu
    {
        static readonly Size ITEM_SIZE = new Size(126,80);
        static readonly int OFFSET_Y = 9;
        static readonly int OFFSET_X = 14;
        private BitmapImage src = new BitmapImage();
        public BitmapImage Src { get { return src.Clone(); } }
        public Size size { get { return new Size(src.PixelWidth,src.PixelHeight); } }
        // Equip  - 4
        // Pistols - 5
        // Mid-Tier - 5
        // Rifles - 5
        // Grenades - 5
        private BuyItem[] items = new BuyItem[4 + 5];

        enum NAMES { KevlarVest, KevlarAndHelmet, Zeus, DefuseKit,Flashbang,Smoke,HE,Molotov,Decoy };
        public AutoBuyMenu()
        {
            src.BeginInit();
            src.UriSource = new Uri("resource\\images\\auto_buy.png", UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            InitBuyItems();
        }

        private void InitBuyItems()
        {
            for (int i = 0; i < 4; i++)
            {
                BuyItem item = new BuyItem(((NAMES)i).ToString(),
                    new Point(OFFSET_X, 43 + OFFSET_Y * i + (ITEM_SIZE.Height * i)), ITEM_SIZE
                    ,false);
                items[i] = item;
            }
            for (int i = 0; i < 5; i++)
            {
                BuyItem item = new BuyItem(((NAMES)4+i).ToString(),
                    new Point(692 + OFFSET_X, 43 + OFFSET_Y * i + (ITEM_SIZE.Height * i)), ITEM_SIZE
                    ,true);
                items[4+i] = item;
            }
        }
        public BuyItem GetItem(Point place)
        {
            for (int i = 0; i < items.Length; i++)
            {
                BuyItem item = items[i];
                Point pos = item.Position;
                Size size = item.Size;
                if (pos.X < place.X && pos.X + size.Width > place.X
                    && pos.Y < place.Y && pos.Y + size.Height > place.Y)
                    return GetItem(i);
            }
            return null;
        }
        public BuyItem GetItem(int index)
        {
            return items[index];
        }

        public BuyItem GetItem(System.Windows.Point pos)
        {
            return GetItem(new Point((int)pos.X, (int)pos.Y));
        }
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
        public BitmapSource GetImage()
        {
            Bitmap copy = BitmapImage2Bitmap(src);
            List<BuyItem> enabled = new List<BuyItem>();
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].IsEnabled())
                    enabled.Add(items[i]);
            }
            if (copy != null) 
            {
                for (int i = 0; i < enabled.Count; i++)
                {
                    BuyItem item = enabled[i];
                    for (int y = item.Position.Y + item.Size.Height / 4; y < item.Size.Height + item.Position.Y; y++)
                    {
                        for (int x = item.Position.X; x < item.Size.Width /(item.IsGrenade() ? 1.7 : 1.5) + item.Position.X; x++)
                        {
                            if (copy.GetPixel(x,y) == Color.FromArgb(65, 65, 65))
                            {
                                copy.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                            }
                        }
                    }
                    
                }
            }
            return Bitmap2BitmapImage(copy);
        }
        private BitmapSource Bitmap2BitmapImage(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapSource retval;

            try
            {
                retval = Imaging.CreateBitmapSourceFromHBitmap(
                             hBitmap,
                             IntPtr.Zero,
                             System.Windows.Int32Rect.Empty,
                             BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }

            return retval;
        }
    }
}
