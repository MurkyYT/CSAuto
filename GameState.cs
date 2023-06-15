using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.AxHost;

namespace CSAuto
{
    public enum Activity
    {
        Menu,
        Playing,
        Textinput
    }
    public enum Team
    {
        T,
        CT
    }
    public enum WeaponType
    {
        Knife,
        Pistol,
        Rifle,
        SniperRifle,
        SubmachineGun,
        MachineGun,
        Shotgun
    }
    public enum WeaponState
    {
        Holstered,
        Active
    }
    public enum Phase
    {
        Live,
        Warmup,
        Intermission,
        Over,
        Freezetime
    }
    public class GameState
    {
        public Player Player { get; internal set; }
        public Match Match { get; internal set; }
        public Round Round { get; internal set; }
        public string MySteamID { get; internal set; }
        private readonly string JSON;
        public GameState(string JSON)
        {
            if (JSON == null)
                return;
            this.JSON = JSON;
            MySteamID = GetMySteamID();
            Match = new Match()
            {
                Phase = GetMatchPhase()
            };
            Round = new Round()
            {
                Phase = GetRoundPhase(),
                CurrentRound = GetRound()
            };
            Player = new Player()
            {
                CurrentActivity = GetActivity(),
                SteamID = GetSteamID(),
                Team = GetTeam(),
                Health = GetHealth(),
                Armor = GetArmor(),
                Money = GetMoney(),
                HasHelmet = GetHelmetState(),
                HasDefuseKit = HasDefuseKit(),
                IsSpectating = CheckIfSpectator()
            };
            Player.SetWeapons(JSON);
        }

        private string GetSteamID()
        {
            string splitStr = JSON.Split(new string[] { "\"player\": {" }, StringSplitOptions.None)[1].Split('}')[0];
            string[] split = splitStr.Split(new string[] { "\"steamid\": \"" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;
            return split[1].Split('"')[0];
        }

        private string GetMySteamID()
        {
            string splitStr = JSON.Split(new string[] { "\"provider\": {" }, StringSplitOptions.None)[1].Split('}')[0];
            string[] split = splitStr.Split(new string[] { "\"steamid\": \"" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;
            return split[1].Split('"')[0];
        }

        private bool HasDefuseKit()
        {
            string splitStr = JSON.Split(new string[] { "\"player\": {" }, StringSplitOptions.None)[1].Split('}')[0];
            string[] split = splitStr.Split(new string[] { "\"defusekit\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return false;
            try
            {
                return bool.Parse(split[1].Split(',')[0]);
            }
            catch { return false; }
        }
        private bool CheckIfSpectator()
        {
            return GetBombState() != null;
        }
        string GetBombState()
        {
            string[] split = JSON.Split(new string[] { "\"bomb\": {" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;

            string state = split[1].Split(new string[] { "\"state\": \"" }, StringSplitOptions.None)[1];
            return state.Split('"')[0];
        }
        private bool GetHelmetState()
        {
            string[] split = JSON.Split(new string[] { "\"helmet\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return false;
            try
            {
                return bool.Parse(split[1].Split(',')[0]);
            }
            catch { return false; }
        }

        private int GetMoney()
        {
            string[] split = JSON.Split(new string[] { "\"money\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            return int.Parse(split[1].Split(',')[0]);
        }

        private int GetArmor()
        {
            string[] split = JSON.Split(new string[] { "\"armor\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            int armor = int.Parse(split[1].Split(',')[0]);
            return armor;
        }

        private int GetHealth()
        {
            string[] split = JSON.Split(new string[] { "\"health\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            int health = int.Parse(split[1].Split(',')[0]);
            return health;
        }

        private Team? GetTeam()
        {
            string[] split = JSON.Split(new string[] { "\"team\": \"" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;
            string team = split[1].Split('"')[0];
            switch (team)
            {
                case "T":
                    return Team.T;
                case "CT":
                    return Team.CT;
            }
            return null;
        }

        private Activity? GetActivity()
        {
            string[] splitted = JSON.Split(new string[] { "\"activity\": \"" }, StringSplitOptions.None);
            if (splitted.Length > 1)
            {
                string activity = splitted[1].Split('"')[0];
                switch (activity)
                {
                    case "menu":
                        return Activity.Menu;
                    case "textinput":
                        return Activity.Textinput;
                    case "playing":
                        return Activity.Playing;
                }
            }
            return null;
        }

        private int GetRound()
        {
            string[] splitted = JSON.Split(new string[] { "\"round\": " }, StringSplitOptions.None);
            if (splitted.Length > 1)
            {
                bool succes = int.TryParse(splitted[1].Split(',')[0], out int res);
                if (succes)
                    return res;
            }
            return -1;
        }
        private Phase? GetRoundPhase()
        {
            string[] split = JSON.Split(new string[] { "\"round\": {" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;
            string state = split[1].Split(new string[] { "\"phase\": \"" }, StringSplitOptions.None)[1].Split('"')[0];
            switch (state)
            {
                case "live":
                    return Phase.Live;
                case "over":
                    return Phase.Over;
                case "freezetime":
                    return Phase.Freezetime;
                default:
                    return null;
            }
        }
        private Phase? GetMatchPhase()
        {
            string[] split = JSON.Split(new string[] { "\"map\": {" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;
            string state = split[1].Split(new string[] { "\"phase\": \"" }, StringSplitOptions.None)[1].Split('"')[0];
            switch (state)
            {
                case "live":
                    return Phase.Live;
                case "warmup":
                    return Phase.Warmup;
                case "intermission":
                    return Phase.Intermission;
                default:
                    return null;
            }
        }
    }
    public class Weapon
    {
        public int Index { get; internal set; }
        public int Bullets { get; internal set; }
        public int ClipSize { get; internal set; }
        public int ReserveBullets { get; internal set; }
        public string Name { get; internal set; }
        public WeaponType? Type { get; internal set; }
        public WeaponState? State { get; internal set; }
    }
    public class Match
    {
        public Phase? Phase { get; internal set; }
    }
    public class Round
    {
        public int CurrentRound { get; internal set; }
        public Phase? Phase { get; internal set; }
    }
    public class Player
    {
        public Weapon ActiveWeapon { get; internal set; }
        public Weapon[] Weapons { get; internal set; }
        public Activity? CurrentActivity { get; internal set; }
        public Team? Team { get; internal set; }
        public int Health { get; internal set; }
        public int Armor { get; internal set; }
        public int Money { get; internal set; }
        public bool HasHelmet { get; internal set; }
        public bool HasDefuseKit { get; internal set; }
        public bool IsSpectating { get; internal set; }
        public string SteamID { get; internal set; }
        internal void SetWeapons(string JSON)
        {
            string weapons = GetWeapons(JSON);
            int amountOfWeapons = CountWeapons(weapons);
            Weapons = new Weapon[amountOfWeapons];
            for (int i = 0; i < amountOfWeapons; i++)
            {
                Weapons[i] = GetWeaponAt(weapons,i);
            }
        }
        private Weapon GetWeaponAt(string weapons,int index)
        {
            string[] splitted = weapons.Split(new string[] { $"\"weapon_{index}\": {{" }, StringSplitOptions.None);
            if(splitted.Length > 1)
            {
                try
                {
                    string weapon = splitted[1].Split('}')[0];
                    string name = weapon.Split(new string[] { "\"name\": \"" }, StringSplitOptions.None)[1].Split('"')[0];
                    string type = weapon.Split(new string[] { "\"type\": \"" }, StringSplitOptions.None)[1].Split('"')[0];
                    string state = weapon.Split(new string[] { "\"state\": \"" }, StringSplitOptions.None)[1].Split('"')[0];
                    int bullets = GetBullets(weapon);
                    int reserveBullets = GetReserveBullets(weapon);
                    int clipSize = GetClipSize(weapon);

                    Weapon res = new Weapon()
                    {
                        Index = index,
                        Name = name,
                        Type = GetTypeOfWeapon(type),
                        State = GetStateOfWeapon(state),
                        Bullets = bullets,
                        ReserveBullets = reserveBullets,
                        ClipSize = clipSize
                    };
                    if (res.State == WeaponState.Active)
                        ActiveWeapon = res;
                    return res;
                }
                catch { return new Weapon()
                {
                    Index = index,
                    Name = "NULL",
                    Type = null,
                    State = null,
                    Bullets = -1,
                    ReserveBullets = -1,
                    ClipSize = -1
                };  }
            }
            throw new IndexOutOfRangeException("Weapon index was out of bounds");
        }

        private int GetClipSize(string weapon)
        {
            if (weapon == null)
                return -1;
            string[] split = weapon.Split(new string[] { "\"ammo_clip_max\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            int bullets = int.Parse(split[1].Split(',')[0]);
            return bullets;
        }

        private int GetReserveBullets(string weapon)
        {
            if (weapon == null)
                return -1;
            string[] split = weapon.Split(new string[] { "\"ammo_reserve\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            int bullets = int.Parse(split[1].Split(',')[0]);
            return bullets;
        }

        private int GetBullets(string weapon)
        {
            if (weapon == null)
                return -1;
            string[] split = weapon.Split(new string[] { "\"ammo_clip\":" }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            int bullets = int.Parse(split[1].Split(',')[0]);
            return bullets;
        }
        private WeaponState? GetStateOfWeapon(string state)
        {
            switch (state)
            {
                case "holstered":
                    return WeaponState.Holstered;
                case "active":
                    return WeaponState.Active;
            }
            return null;
        }

        private WeaponType? GetTypeOfWeapon(string type)
        {
            switch (type)
            {
                case "Knife":
                    return WeaponType.Knife;
                case "Pistol":
                    return WeaponType.Pistol;
                case "Rifle":
                    return WeaponType.Rifle;
                case "Sniper Rifle":
                    return WeaponType.SniperRifle;
                case "Submachine Gun":
                    return WeaponType.SubmachineGun;
                case "Machine Gun":
                    return WeaponType.MachineGun;
                case "Shotgun":
                    return WeaponType.Shotgun;
            }
            return null;
        }

        private int CountWeapons(string weapons)
        {
            if (weapons == null)
                return 0;
            int count = 0;
            int a = 0;
            while ((a = weapons.IndexOf($"weapon_{count}", a)) != -1)
            {
                a += "weapon_".Length;
                count++;
            }
            return count;
        }
        private string GetWeapons(string jSON)
        {
            string[] splitted = jSON.Split(new string[] { "\"weapons\": {" }, StringSplitOptions.None);
            if (splitted.Length > 1)
            {
                string weapons = splitted[1].Split(new string[] { "},\r\n\t\t\"match_stats\": {" }, StringSplitOptions.None)[0];
                return weapons;
            }
            return null;
        }
    }
}
