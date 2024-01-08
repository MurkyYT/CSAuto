using Murky.Utils;
using Newtonsoft.Json;
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
    public class BuyItem : IComparable<BuyItem>
    {
        [JsonProperty]
        private bool isEnabled;
        [JsonProperty]
        private AutoBuyMenu.NAMES name;
        [JsonProperty]
        private Point position;
        [JsonProperty]
        private Size size;
        [JsonProperty]
        private bool isGrenade;
        [JsonProperty]
        private int priority;
        public BuyItem(AutoBuyMenu.NAMES name, Point position, Size size, bool isGrenade)
        {
            this.name = name;
            this.position = position;
            this.size = size;
            isEnabled = false;
            this.isGrenade = isGrenade;
        }
        public void SetEnabled(bool isEnabled) { this.isEnabled = isEnabled; }
        public void SetPriority(int priority) { this.priority = priority; }
        public int GetPriority() { return priority; }
        public bool IsEnabled() { return isEnabled; }
        public bool IsGrenade() { return isGrenade; }

        public int CompareTo(BuyItem other)
        {
            return priority.CompareTo(other.priority);
        }

        [JsonIgnore]
        public AutoBuyMenu.NAMES Name { get { return name; } }
        [JsonIgnore]
        public Point Position { get { return position; } }
        [JsonIgnore]
        public Size Size { get { return size; } }
    }
    public class AutoBuyMenu
    {
        static readonly Size ITEM_SIZE = new Size(126,80);
        static readonly int OFFSET_Y = 9;
        static readonly int OFFSET_X = 14;
        private BitmapImage src = new BitmapImage();
        private Color[] colors = new Color[16];
        public BitmapImage Src { get { return src.Clone(); } }
        public Size size { get { return new Size(src.PixelWidth,src.PixelHeight); } }
        // Equip  - 4
        // Pistols - 5
        // Mid-Tier - 5
        // Rifles - 5
        // Grenades - 5
        const int AMOUNT = 4 + 5;
        private List<BuyItem> items = new List<BuyItem>();

        public enum NAMES { KevlarVest, KevlarAndHelmet, Zeus, DefuseKit,Flashbang,Smoke,HE,Molotov,Decoy };
        public AutoBuyMenu()
        {
            src.BeginInit();
            src.UriSource = new Uri("resource\\images\\auto_buy.png", UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.FromArgb(50 + i, 50 + i, 50 + i);
        }

        private void InitBuyItems()
        {
            for (int i = 0; i < 4; i++)
            {
                BuyItem item = new BuyItem(((NAMES)i),
                    new Point(OFFSET_X, 43 + OFFSET_Y * i + (ITEM_SIZE.Height * i)), ITEM_SIZE
                    ,false);
                items.Add(item);
            }
            for (int i = 0; i < 5; i++)
            {
                BuyItem item = new BuyItem(((NAMES)4+i),
                    new Point(692 + OFFSET_X, 43 + OFFSET_Y * i + (ITEM_SIZE.Height * i)), ITEM_SIZE
                    ,true);
                items.Add(item);
            }
        }
        public BuyItem GetItem(Point place)
        {
            for (int i = 0; i < items.Count; i++)
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
        public BuyItem GetItem(NAMES itemName)
        {
            for (int i = 0; i < items.Count; i++)
            {
                BuyItem item = items[i];
                if(item.Name == itemName) return item;
            }
            return null;
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
            using (Bitmap copy = BitmapImage2Bitmap(src))
            {
                BuyItem[] enabled = GetEnabled();
                if (copy != null)
                {
                    for (int i = 0; i < enabled.Length; i++)
                    {
                        BuyItem item = enabled[i];
                        for (int y = item.Position.Y + item.Size.Height / 4; y < item.Size.Height + item.Position.Y; y++)
                        {
                            for (int x = item.Position.X; x < item.Size.Width / (item.IsGrenade() ? 1.72 : 1.485) + item.Position.X; x++)
                            {
                                Color pixelColor = copy.GetPixel(x, y);
                                if (colors.Contains(pixelColor))
                                {
                                    copy.SetPixel(x, y, Color.FromArgb(255 - ((65 - pixelColor.R) * 10), 255 - ((65 - pixelColor.G) * 10), 255 - ((65 - pixelColor.B) * 10)));
                                }
                            }
                        }

                    }
                }
                items.Sort();
                return Bitmap2BitmapImage(copy);
            }
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
        public void Save(RegistrySettings settings)
        {
            settings.Set("AutoBuyConfig",JsonConvert.SerializeObject(items));
        }
        public void Load(RegistrySettings settings) 
        {
            InitBuyItems();
            if (settings["AutoBuyConfig"] != null)
            {
                Newtonsoft.Json.Linq.JArray ar = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(settings["AutoBuyConfig"]);
                BuyItem[] temp = ar.ToObject<BuyItem[]>();
                for (int i = 0; i < temp.Length && i < AMOUNT; i++)
                    items[i] = temp[i];
            }
        }
        public static BuyItem[] GetEnabled(RegistrySettings settings)
        {
            List<BuyItem> enabled = new List<BuyItem>();
            if (settings["AutoBuyConfig"] != null)
            {
                Newtonsoft.Json.Linq.JArray ar = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(settings["AutoBuyConfig"]);
                BuyItem[] temp = ar.ToObject<BuyItem[]>();
                for (int i = 0; i < temp.Length && i < AMOUNT; i++)
                    if (temp[i].IsEnabled())
                        enabled.Add(temp[i]);
            }
            return enabled.ToArray();
        }
        public BuyItem[] GetEnabled()
        {
            List<BuyItem> enabled = new List<BuyItem>();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].IsEnabled())
                    enabled.Add(items[i]);
            }
            return enabled.ToArray();
        }
    }
}
