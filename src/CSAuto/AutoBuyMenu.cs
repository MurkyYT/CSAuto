using ControlzEx.Theming;
using CSAuto.Properties;
using Murky.Utils;
using Murky.Utils.CSGO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

        public BuyItem Copy() => MemberwiseClone() as BuyItem;
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
        public new CustomBuyItem Copy() => MemberwiseClone() as CustomBuyItem;
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
        private List<BuyItem> ctItems = new List<BuyItem>();
        private List<BuyItem> tItems = new List<BuyItem>();
        private List<CustomBuyItem> ctCustomItems = new List<CustomBuyItem>();
        private List<CustomBuyItem> tCustomItems = new List<CustomBuyItem>();
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
            Nova,
            MP7,
            Negev,
            MAG7,
            PPBizon,
            UMP45,
            M249,
            XM1014,
            MP5SD,
            P90,
            MP9,
            MAC10,
            SawedOff,
            Famas,
            GalilAR,
            M4A4,
            M4A1S,
            AK47,
            AUG,
            SG553,
            SSG08,
            AWP,
            SCAR20,
            G3SG1
        }
        private readonly Dictionary<NAMES, object[]> weaponsInfo = new Dictionary<NAMES, object[]>()
        {
            { NAMES.G3SG1,new object[] { "weapon_g3sg1", 5000 } },
            { NAMES.SCAR20,new object[] { "weapon_scar20", 5000 } },
            { NAMES.AWP,new object[] { "weapon_awp", 4750 } },
            { NAMES.SSG08,new object[] { "weapon_ssg08", 1700 } },
            { NAMES.SG553,new object[] { "weapon_sg556", 3000 } },
            { NAMES.AUG,new object[] { "weapon_aug", 3300 } },
            { NAMES.AK47,new object[] { "weapon_ak47", 2700 } },
            { NAMES.M4A1S,new object[] { "weapon_m4a1_silencer", 2900 } },
            { NAMES.M4A4,new object[] { "weapon_m4a1", 3000 } },
            { NAMES.GalilAR,new object[] { "weapon_galilar", 1800 } },
            { NAMES.Famas,new object[] { "weapon_famas", 2050 } },
            { NAMES.SawedOff,new object[] { "weapon_sawedoff", 1100 } },
            { NAMES.MAC10,new object[] { "weapon_mac_10", 1050 } },
            { NAMES.R8Revolver,new object[] { "weapon_revolver", 600 } },
            { NAMES.CZ75Auto,new object[] { "weapon_cz75a", 500 } },
            { NAMES.Deagle,new object[] { "weapon_deagle", 700 } },
            { NAMES.FiveSeven,new object[] { "weapon_fiveseven", 500 } },
            { NAMES.Tec9,new object[] { "weapon_tec9", 500 } },
            { NAMES.DualBerettas,new object[] { "weapon_elite", 300 } },
            { NAMES.P250,new object[] { "weapon_p250", 300 } },
            { NAMES.Glock18,new object[] { "weapon_glock", 200 } },
            { NAMES.Zeus,new object[] { "weapon_taser", 200 } },
            { NAMES.Flashbang,new object[] { "weapon_flashbang", 200 } },
            { NAMES.HE,new object[] { "weapon_hegrenade", 300 } },
            { NAMES.Smoke,new object[] { "weapon_smokegrenade", 300 } },
            { NAMES.Decoy,new object[] { "weapon_decoy", 50 } },
            { NAMES.Nova,new object[] { "weapon_nova", 1050 } },
            { NAMES.MP7,new object[] { "weapon_mp7", 1500 } },
            { NAMES.Negev,new object[] { "weapon_negev", 1700 } },
            { NAMES.MAG7,new object[] { "weapon_mag7", 1300 } },
            { NAMES.PPBizon,new object[] { "weapon_bizon", 1400 } },
            { NAMES.UMP45,new object[] { "weapon_ump45", 1200 } },
            { NAMES.M249,new object[] { "weapon_m249", 5200 } },
            { NAMES.XM1014,new object[] { "weapon_xm1014", 2000 } },
            { NAMES.MP5SD,new object[] { "weapon_mp5sd", 1500 } },
            { NAMES.P90,new object[] { "weapon_p90", 2350 } },
            { NAMES.MP9,new object[] { "weapon_mp9", 1250 } }
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
                BuyItem item = new BuyItem(((NAMES)i + 1),
                    new Point(OFFSET_X, 43 + (OFFSET_Y * i) + (SMALL_ITEM_SIZE.Height * i)), SMALL_ITEM_SIZE
                    ,false,
                    $"1{i+1}");
                if (i != 3)
                    tItems.Add(item);
                ctItems.Add(item.Copy());
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
                CustomBuyItem item = new CustomBuyItem(NAMES.None,
                    new Point(152 + OFFSET_X, 43 + (OFFSET_Y * i) + (SMALL_ITEM_SIZE.Height * i)), SMALL_ITEM_SIZE, $"2{i+1}", new NAMES[]
                    { NAMES.None, NAMES.R8Revolver,NAMES.CZ75Auto,NAMES.DualBerettas,NAMES.P250,NAMES.FiveSeven,NAMES.Deagle }, new NAMES[]
                    { NAMES.None, NAMES.R8Revolver,NAMES.CZ75Auto,NAMES.DualBerettas,NAMES.P250,NAMES.Tec9,NAMES.Deagle });
                tCustomItems.Add(item);
                ctCustomItems.Add(item.Copy());
            }
            //Mid-tier
            for (int i = 0; i < 5; i++)
            {
                CustomBuyItem item = new CustomBuyItem(NAMES.None,
                    new Point(304 + OFFSET_X, 43 + (OFFSET_Y * i) + (MIDTIER_ITEM_SIZE.Height * i)), MIDTIER_ITEM_SIZE, $"3{i + 1}", new NAMES[]
                    {  NAMES.None, NAMES.Nova,NAMES.MP7,NAMES.MAG7,NAMES.Negev,NAMES.PPBizon,NAMES.UMP45,NAMES.M249,NAMES.XM1014,NAMES.MP5SD,NAMES.P90,NAMES.MP9 }, new NAMES[]
                    {  NAMES.None, NAMES.Nova,NAMES.MP7,NAMES.SawedOff,NAMES.Negev,NAMES.PPBizon,NAMES.UMP45,NAMES.M249,NAMES.XM1014,NAMES.MP5SD,NAMES.P90,NAMES.MAC10 });
                tCustomItems.Add(item);
                ctCustomItems.Add(item.Copy());
            }
            //Rifles
            for (int i = 0; i < 5; i++)
            {
                CustomBuyItem item = new CustomBuyItem(NAMES.None,
                    new Point(490 + OFFSET_X, 43 + (OFFSET_Y * i) + (RIFLES_ITEM_SIZE.Height * i)), RIFLES_ITEM_SIZE, $"4{i + 1}", new NAMES[]
                    {  NAMES.None, NAMES.Famas,NAMES.M4A4,NAMES.M4A1S,NAMES.AUG,NAMES.SSG08,NAMES.SCAR20,NAMES.AWP }, new NAMES[]
                    {  NAMES.None, NAMES.GalilAR,NAMES.AK47,NAMES.SG553,NAMES.SSG08,NAMES.G3SG1,NAMES.AWP });
                tCustomItems.Add(item);
                ctCustomItems.Add(item.Copy());
            }
            //Grenades
            for (int i = 0; i < 5; i++)
            {
                BuyItem item = new BuyItem(((NAMES)5 + i),
                    new Point(692 + OFFSET_X, 43 + (OFFSET_Y * i) + (SMALL_ITEM_SIZE.Height * i)), SMALL_ITEM_SIZE
                    , true,
                    $"5{i + 1}");
                tItems.Add(item);
                ctItems.Add(item.Copy());
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
                enc.Frames.Add(BitmapFrame.Create(bitmapImage,null,null,null));
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
                return Bitmap2BitmapImage(copy);
            }
        }

        private void ColorEnabled(Bitmap copy, BuyItem[] enabled)
        {
            for (int i = 0; i < enabled.Length; i++)
            {
                BuyItem item = enabled[i];
                for (int y = item.Position.Y + (item.Size.Height / 4); y < item.Size.Height + item.Position.Y - (item.IsGrenade() ? 0 : 18); y++)
                {
                    for (int x = item.Position.X; x < (item.Size.Width / (item.IsGrenade() ? 1.72 : 1)) + item.Position.X; x++)
                    {
                        Color pixelColor = NativeMethods.GetPixel(copy,x,y);
                        if (colors.Contains(pixelColor))
                        {
                            System.Windows.Media.Color accent = ThemeManager.Current.GetTheme($"Dark.{Settings.Default.currentColor}").PrimaryAccentColor;
                            float ratio = (float)(accent.R + accent.G + accent.B)/(pixelColor.R + pixelColor.G + pixelColor.B) * 2;
                            NativeMethods.ReplaceColor(copy, x, y,
                                Color.FromArgb(
                                    Math.Max((int)(accent.R - ((65 - pixelColor.R) * ratio)),0),
                                    Math.Max((int)(accent.G - ((65 - pixelColor.G) * ratio)),0),
                                    Math.Max((int)(accent.B - ((65 - pixelColor.B) * ratio)),0)));
                        }
                    }
                }
            }
        }
        //public void CopyRegionIntoImage(Bitmap srcBitmap, RectangleF srcRegion, ref Bitmap destBitmap, RectangleF destRegion)
        //{
        //    using (Graphics grD = Graphics.FromImage(destBitmap))
        //    {
        //        grD.DrawImage(srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);
        //    }
        //}
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
                            GraphicsUnit units = GraphicsUnit.Pixel;
                            Rectangle destReg = new Rectangle()
                            {
                                Width = customItem.Size.Width - 25,
                                Height = customItem.Size.Height,
                                X = customItem.Position.X + 25,
                                Y = customItem.Position.Y
                            };
                            RectangleF srcRegF = customImg.GetBounds(ref units);
                            Rectangle srcReg = new Rectangle()
                            {
                                Width = (int)srcRegF.Size.Width,
                                Height = (int)srcRegF.Size.Height,
                                X = (int)srcRegF.X,
                                Y = (int)srcRegF.Y,
                            };
                            srcReg.Width -= 25;
                            srcReg.X += 25;
                            NativeMethods.CopyRegionIntoImage(customImg, srcReg, ref copy, destReg);
                            //CopyRegionIntoImage(customImg, srcReg, ref copy, destReg);
                        }
                    }
                    catch
                    {
                        Log.WriteLine($"|AutoBuyMenu.cs| weapon_{customItem.Name.ToString().ToLower()}.png isn't found");
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
            if (settings["CTCustomAutoBuyConfig"] != null)
                ctCustomItems = JsonConvert.DeserializeObject<List<CustomBuyItem>>(settings["CTCustomAutoBuyConfig"]);
            if (settings["CTAutoBuyConfig"] != null)
                ctItems = JsonConvert.DeserializeObject<List<BuyItem>>(settings["CTAutoBuyConfig"]);
            
            if (settings["TAutoBuyConfig"] != null)
                tItems = JsonConvert.DeserializeObject<List<BuyItem>>(settings["TAutoBuyConfig"]);
            if (settings["TCustomAutoBuyConfig"] != null)
                tCustomItems = JsonConvert.DeserializeObject<List<CustomBuyItem>>(settings["TCustomAutoBuyConfig"]);
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
            List<CustomBuyItem> customItems = isCt ? ctCustomItems : tCustomItems;
            List<BuyItem> enabled = new List<BuyItem>();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].IsEnabled())
                    enabled.Add(items[i]);
            }
            for (int i = 0; i < customItems.Count; i++)
            {
                if (customItems[i].IsEnabled())
                    enabled.Add(customItems[i]);
            }
            enabled.Sort();
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

        public List<BuyItem> GetItemsToBuy(GameState gameState,int armorAmountToRebuy)
        {
            BuyItem[] items = GetEnabled(gameState.Player.Team == Team.CT);
            int armor = gameState.Player.Armor;
            bool hasHelmet = gameState.Player.HasHelmet;
            bool hasDefuseKit = gameState.Player.HasDefuseKit;
            int money = gameState.Player.Money;
            bool hasSmoke = gameState.Player.HasWeapon("weapon_smokegrenade");
            bool hasHE = gameState.Player.HasWeapon("weapon_hegrenade");
            bool hasDecoy = gameState.Player.HasWeapon("weapon_decoy");
            bool hasFlash = gameState.Player.HasWeapon("weapon_flashbang");
            bool hasP2000 = gameState.Player.HasWeapon("weapon_hkp2000");
            bool hasUSP = gameState.Player.HasWeapon("weapon_usp_silencer");
            bool hasMolotov =
                gameState.Player.HasWeapon("weapon_molotov")
                ||
                gameState.Player.HasWeapon("weapon_incgrenade");
            int grenadeCount = 0
                + (hasSmoke ? 1 : 0)
                + (hasHE ? 1 : 0)
                + (hasDecoy ? 1 : 0)
                + (hasFlash ? 1 : 0)
                + (hasMolotov ? 1 : 0);
            List<BuyItem> res = new List<BuyItem>();
            foreach (BuyItem item in items)
            {
                switch (item.Name)
                {
                    case NAMES.KevlarVest:
                        {
                            if (money >= 650 && armor <= armorAmountToRebuy && !hasHelmet)
                            {
                                res.Add(item);
                                money -= 650;
                                armor = 100;
                            }
                        }
                        break;
                    case NAMES.KevlarAndHelmet:
                        {
                            if (money >= 350 && armor == 100 && !hasHelmet)
                            {
                                res.Add(item);
                                money -= 350;
                                hasHelmet = true;
                            }
                            else if (money >= 1000 && armor <= armorAmountToRebuy && !hasHelmet)
                            {
                                res.Add(item);
                                money -= 1000;
                                armor = 100;
                                hasHelmet = true;
                            }
                        }
                        break;
                    case NAMES.DefuseKit:
                        {
                            if (money >= 400 && !hasDefuseKit)
                            {
                                res.Add(item);
                                money -= 400;
                                hasDefuseKit = true;
                            }
                        }
                        break;
                    case NAMES.USP_S:
                        {
                            if (money >= 200 && !hasUSP && !hasP2000)
                            {
                                money -= 200;
                                res.Add(item);
                                hasUSP = true;
                                hasP2000 = true;
                            }
                        }
                        break;
                    case NAMES.Molotov:
                        {
                            if (money >= 400 && !hasMolotov && gameState.Player.Team == Team.T && grenadeCount < 4)
                            {
                                res.Add(item);
                                money -= 400;
                                hasMolotov = true;
                                grenadeCount++;
                            }
                            if (money >= 500 && !hasMolotov && gameState.Player.Team == Team.CT && grenadeCount < 4)
                            {
                                res.Add(item);
                                money -= 500;
                                hasMolotov = true;
                                grenadeCount++;
                            }
                        }
                        break;
                    default:
                        object[] info = weaponsInfo[item.Name];
                        int price = (int)info[1];
                        string weaponName = (string)info[0];
                        if (money >= price && !gameState.Player.HasWeapon(weaponName))
                        {
                            if (item.IsGrenade())
                            {
                                if (grenadeCount < 4)
                                {
                                    money -= price;
                                    res.Add(item);
                                    grenadeCount++;
                                }
                            }
                            else
                            {
                                money -= price;
                                res.Add(item);
                            }
                        }
                        break;
                }
            }
            return res;
        }
    }
}
