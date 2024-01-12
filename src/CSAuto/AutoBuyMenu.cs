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
        [JsonProperty]
        private string slot;
        public BuyItem(AutoBuyMenu.NAMES name, Point position, Size size, bool isGrenade, string slot)
        {
            this.name = name;
            this.position = position;
            this.size = size;
            isEnabled = false;
            this.isGrenade = isGrenade;
            this.slot = slot;
        }
        public void SetEnabled(bool isEnabled) { this.isEnabled = isEnabled; }
        public void SetPriority(int priority) { this.priority = priority; }
        public void SetName(AutoBuyMenu.NAMES name) { this.name = name; }
        public AutoBuyMenu.NAMES GetName() => name;
        public int GetPriority() { return priority; }
        public bool IsEnabled() { return isEnabled; }
        public bool IsGrenade() { return isGrenade; }
        public string GetSlot() { return slot; }

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
    public class CustomBuyItem : BuyItem
    {
        [JsonProperty]
        private AutoBuyMenu.NAMES[] ctOptions;
        [JsonProperty]
        private AutoBuyMenu.NAMES[] tOptions;
        public CustomBuyItem(AutoBuyMenu.NAMES name, Point position, Size size, string slot, AutoBuyMenu.NAMES[] ctOptions, AutoBuyMenu.NAMES[] tOptions)
            : base(name, position, size, false,slot)
        {
            this.ctOptions = ctOptions;
            this.tOptions = tOptions;
        }
        public AutoBuyMenu.NAMES[] GetCTOptions() { return ctOptions; }
        public AutoBuyMenu.NAMES[] GetTOptions() { return tOptions; }
    }
    public class AutoBuyMenu
    {
        static readonly Size SMALL_ITEM_SIZE = new Size(126,80);
        static readonly Size MIDTIER_ITEM_SIZE = new Size(159, 80);
        static readonly Size RIFLES_ITEM_SIZE = new Size(175, 80);
        static readonly int OFFSET_Y = 9;
        static readonly int OFFSET_X = 14;
        private BitmapImage ctSrc = new BitmapImage();
        private BitmapImage tSrc = new BitmapImage();
        private Color[] colors = new Color[16];
        public Size size { get { return new Size(ctSrc.PixelWidth, ctSrc.PixelHeight); } } 
        // Equip  - 4
        // Pistols - 5
        // Mid-Tier - 5
        // Rifles - 5
        // Grenades - 5
        private readonly List<BuyItem> ctItems = new List<BuyItem>();
        private readonly List<BuyItem> tItems = new List<BuyItem>();
        private readonly List<CustomBuyItem> ctCustomItems = new List<CustomBuyItem>();
        private readonly List<CustomBuyItem> tCustomItems = new List<CustomBuyItem>();

        public enum NAMES {
            None,
            KevlarVest, 
            KevlarAndHelmet, 
            Zeus, 
            DefuseKit,
            Flashbang,
            Smoke,
            HE,
            Molotov,
            Decoy,
            USP_S,
            Glock18,
            R8Revolver,
            CZ75Auto,
            DualBerettas,
            P250,
            Tec9,
            FiveSeven,
            Deagle,
        };

        public AutoBuyMenu()
        {
            ctSrc = ImageLoader.LoadBitmapImage("ct_auto_buy.png");
            tSrc = ImageLoader.LoadBitmapImage("t_auto_buy.png");
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.FromArgb(50 + i, 50 + i, 50 + i);
        }

        private void InitBuyItems()
        {
            //Equipment
            for (int i = 0; i < 4; i++)
            {
                BuyItem itemT = new BuyItem(((NAMES)i + 1),
                    new Point(OFFSET_X, 43 + OFFSET_Y * i + (SMALL_ITEM_SIZE.Height * i)), SMALL_ITEM_SIZE
                    ,false,
                    $"1{i+1}");
                BuyItem itemCt = new BuyItem(((NAMES)i + 1),
                    new Point(OFFSET_X, 43 + OFFSET_Y * i + (SMALL_ITEM_SIZE.Height * i)), SMALL_ITEM_SIZE
                    , false,
                    $"1{i + 1}");
                if (i != 3)
                    tItems.Add(itemT);
                ctItems.Add(itemCt);
            }
            //Grenades
            for (int i = 0; i < 5; i++)
            {
                BuyItem itemCt = new BuyItem(((NAMES)5+i),
                    new Point(692 + OFFSET_X, 43 + OFFSET_Y * i + (SMALL_ITEM_SIZE.Height * i)), SMALL_ITEM_SIZE
                    ,true,
                    $"5{i + 1}");
                BuyItem itemT = new BuyItem(((NAMES)5 + i),
                    new Point(692 + OFFSET_X, 43 + OFFSET_Y * i + (SMALL_ITEM_SIZE.Height * i)), SMALL_ITEM_SIZE
                    , true,
                    $"5{i + 1}");
                tItems.Add(itemCt);
                ctItems.Add(itemT);
            }
            // Pistols
            BuyItem usp_s = new BuyItem(NAMES.USP_S,
                   new Point(152 + OFFSET_X, 43), SMALL_ITEM_SIZE,false, "21");
            ctItems.Add(usp_s);
            BuyItem glock18 = new BuyItem(NAMES.Glock18,
                   new Point(152 + OFFSET_X, 43), SMALL_ITEM_SIZE, false, "21");
            tItems.Add(glock18);
            for (int i = 1; i < 5; i++)
            {
                CustomBuyItem itemT = new CustomBuyItem(NAMES.None,
                    new Point(152 + OFFSET_X, 43 + OFFSET_Y * i + (SMALL_ITEM_SIZE.Height * i)), SMALL_ITEM_SIZE, $"2{i+1}", new NAMES[]
                    { NAMES.None, NAMES.R8Revolver,NAMES.CZ75Auto,NAMES.DualBerettas,NAMES.P250,NAMES.FiveSeven,NAMES.Deagle }, new NAMES[]
                    { NAMES.None, NAMES.R8Revolver,NAMES.CZ75Auto,NAMES.DualBerettas,NAMES.P250,NAMES.Tec9,NAMES.Deagle });
                CustomBuyItem itemCt = new CustomBuyItem(NAMES.None,
                   new Point(152 + OFFSET_X, 43 + OFFSET_Y * i + (SMALL_ITEM_SIZE.Height * i)), SMALL_ITEM_SIZE, $"2{i + 1}", new NAMES[]
                   { NAMES.None, NAMES.R8Revolver,NAMES.CZ75Auto,NAMES.DualBerettas,NAMES.P250,NAMES.FiveSeven,NAMES.Deagle }, new NAMES[]
                   { NAMES.None, NAMES.R8Revolver,NAMES.CZ75Auto,NAMES.DualBerettas,NAMES.P250,NAMES.Tec9,NAMES.Deagle });
                ctItems.Add(itemCt);
                tItems.Add(itemT);
                tCustomItems.Add(itemT);
                ctCustomItems.Add(itemCt);
            }
        }
        public BuyItem GetItem(Point place,bool isCt)
        {
            List<CustomBuyItem> customItems = isCt ? ctCustomItems : tCustomItems;
            for (int i = 0; i < customItems.Count; i++)
            {
                CustomBuyItem item = customItems[i];
                Point pos = item.Position;
                Size size = item.Size;
                if (pos.X < place.X && pos.X + size.Width > place.X
                    && pos.Y < place.Y && pos.Y + size.Height > place.Y)
                    return GetItem(i, isCt, true);
            }
            List<BuyItem> items = isCt ? ctItems : tItems;
            for (int i = 0; i < items.Count; i++)
            {
                BuyItem item = items[i];
                Point pos = item.Position;
                Size size = item.Size;
                if (pos.X < place.X && pos.X + size.Width > place.X
                    && pos.Y < place.Y && pos.Y + size.Height > place.Y)
                    return GetItem(i,isCt,false);
            }
            return null;
        }
        BuyItem GetItem(int index, bool isCt,bool isCustom)
        {
            if(isCustom)
                return isCt ? ctCustomItems[index] : tCustomItems[index];
            return isCt ? ctItems[index] : tItems[index];
        }
        public BuyItem GetItem(NAMES itemName, bool isCt)
        {
            List<BuyItem> items = isCt ? ctItems : tItems;
            for (int i = 0; i < items.Count; i++)
            {
                BuyItem item = items[i];
                if(item.Name == itemName) return item;
            }
            return null;
        }

        public BuyItem GetItem(System.Windows.Point pos,bool isCt)
        {
            return GetItem(new Point((int)pos.X, (int)pos.Y),isCt);
        }
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
        public BitmapSource GetImage(bool isCt)
        {
            BitmapImage img = isCt ? ctSrc : tSrc;
            List<CustomBuyItem> customItems = isCt ? ctCustomItems : tCustomItems;
            using (Bitmap copy = BitmapImage2Bitmap(img))
            {
                BuyItem[] enabled = GetEnabled(isCt);
                LoadCustomImages(customItems, copy);
                ColorEnabled(copy, enabled);
                (isCt ? ctItems : tItems).Sort();
                return Bitmap2BitmapImage(copy);
            }
        }

        private void ColorEnabled(Bitmap copy, BuyItem[] enabled)
        {
            for (int i = 0; i < enabled.Length; i++)
            {
                BuyItem item = enabled[i];
                for (int y = item.Position.Y + item.Size.Height / 4; y < item.Size.Height + item.Position.Y - (item.IsGrenade() ? 0 : 18); y++)
                {
                    for (int x = item.Position.X; x < item.Size.Width / (item.IsGrenade() ? 1.72 : 1) + item.Position.X; x++)
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

        private void LoadCustomImages(List<CustomBuyItem> customItems, Bitmap copy)
        {
            foreach (CustomBuyItem customItem in customItems)
            {
                if (customItem.Name != NAMES.None)
                {
                    try
                    {
                        using (Bitmap customImg = BitmapImage2Bitmap(ImageLoader.LoadBitmapImage($"weapon_{customItem.Name.ToString().ToLower()}.png")))
                        {
                            for (int y = customItem.Position.Y; y < customItem.Size.Height + customItem.Position.Y; y++)
                            {
                                for (int x = customItem.Position.X + 25; x < customItem.Size.Width + customItem.Position.X; x++)
                                {
                                    copy.SetPixel(x, y,
                                        customImg.GetPixel(x - customItem.Position.X
                                        , y - customItem.Position.Y));
                                }
                            }
                        }
                    }
                    catch
                    {
                        Log.WriteLine($"weapon_{customItem.Name.ToString().ToLower()}.png isn't found");
                    }
                }
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
            settings.Set("CTAutoBuyConfig", JsonConvert.SerializeObject(ctItems));
            settings.Set("CTCustomAutoBuyConfig", JsonConvert.SerializeObject(ctCustomItems));
            settings.Set("TAutoBuyConfig", JsonConvert.SerializeObject(tItems));
            settings.Set("TCustomAutoBuyConfig", JsonConvert.SerializeObject(tCustomItems));
        }
        public void Load(RegistrySettings settings) 
        {
            InitBuyItems();
            if (settings["CTAutoBuyConfig"] != null)
            {
                Newtonsoft.Json.Linq.JArray ar = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(settings["CTAutoBuyConfig"]);
                BuyItem[] temp = ar.ToObject<BuyItem[]>();
                for (int i = 0; i < temp.Length && i < ctItems.Count; i++)
                    ctItems[i] = temp[i];
            }
            if (settings["CTCustomAutoBuyConfig"] != null)
            {
                Newtonsoft.Json.Linq.JArray ar = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(settings["CTCustomAutoBuyConfig"]);
                CustomBuyItem[] temp = ar.ToObject<CustomBuyItem[]>();
                for (int i = 0; i < temp.Length && i < ctCustomItems.Count; i++)
                    ctCustomItems[i] = temp[i];
            }
            if (settings["TAutoBuyConfig"] != null)
            {
                Newtonsoft.Json.Linq.JArray ar = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(settings["TAutoBuyConfig"]);
                BuyItem[] temp = ar.ToObject<BuyItem[]>();
                for (int i = 0; i < temp.Length && i < tItems.Count; i++)
                    tItems[i] = temp[i];
            }
            if (settings["TCustomAutoBuyConfig"] != null)
            {
                Newtonsoft.Json.Linq.JArray ar = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(settings["TCustomAutoBuyConfig"]);
                CustomBuyItem[] temp = ar.ToObject<CustomBuyItem[]>();
                for (int i = 0; i < temp.Length && i < tCustomItems.Count; i++)
                    tCustomItems[i] = temp[i];
            }
        }
        public static BuyItem[] GetEnabled(RegistrySettings settings,bool isCt)
        {
            List<BuyItem> enabled = new List<BuyItem>();
            if (settings[isCt ? "CTAutoBuyConfig" : "TAutoBuyConfig"] != null)
            {
                Newtonsoft.Json.Linq.JArray ar = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(settings[isCt ? "CTAutoBuyConfig" : "TAutoBuyConfig"]);
                BuyItem[] temp = ar.ToObject<BuyItem[]>();
                for (int i = 0; i < temp.Length && i < temp.Length; i++)
                    if (temp[i].IsEnabled())
                        enabled.Add(temp[i]);
            }
            return enabled.ToArray();
        }
        public BuyItem[] GetEnabled(bool isCt)
        {
            List<BuyItem> items = isCt ? ctItems : tItems;
            List<BuyItem> enabled = new List<BuyItem>();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].IsEnabled())
                    enabled.Add(items[i]);
            }
            return enabled.ToArray();
        }

        public bool ContainsCustom(bool isCt,NAMES name)
        {
            List<CustomBuyItem> customItems = isCt ? ctCustomItems : tCustomItems;
            foreach(CustomBuyItem customItem in customItems)
            {
                if (name == customItem.GetName())
                    return true;
            }
            return false;
        }
    }
}
