﻿using System;

namespace Murky.Utils.CSGO
{
    public enum BombState
    {
        Planted,
        Exploded,
        Defused
    }
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
        Shotgun,
        Grenade
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
    public enum Mode
    {
        Competitive,
        Casual,
        Deathmatch,
        Wingman,
        Demolition,
        ArmsRace,
        COOP,
        Custom,
        DangerZone
    }
    public class GameState : IDisposable
    {
        public Player Player { get; private set; }
        public Match Match { get; private set; }
        public Round Round { get; private set; }
        public long Timestamp { get; private set; }
        public string MySteamID { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsSpectating { get; private set; }
        public string JSON { get { return _JSON; } }
        private string _JSON;
        internal void UpdateJson(string JSON)
        {
            if (JSON == null)
                return;
            _JSON = JSON;
            MySteamID = GetMySteamID();
            Timestamp = GetTimeStamp();
            Match = new Match()
            {
                Phase = GetMatchPhase(),
                Mode = GetMode(),
                TScore = GetTScore(),
                CTScore = GetCTScore(),
                Map = GetMap()
            };
            Round = new Round()
            {
                Phase = GetRoundPhase(),
                CurrentRound = GetRound(),
                Bombstate = GetRoundBombState()
            };
            if (HasPlayer())
            {
                Player = new Player()
                {
                    Name = GetPlayerName(),
                    CurrentActivity = GetActivity(),
                    SteamID = GetSteamID(),
                    Team = GetTeam(),
                    Health = GetHealth(),
                    Armor = GetArmor(),
                    Money = GetMoney(),
                    Kills = GetPlayerKills(),
                    Deaths = GetPlayerDeaths(),
                    MVPS = GetPlayerMVPS(),
                    HasHelmet = GetHelmetState(),
                    HasDefuseKit = HasDefuseKit()
                };
                Player.SetWeapons(JSON);
                IsDead = Player.SteamID != MySteamID;
            }
            IsSpectating = CheckIfSpectator();
        }

        private int GetPlayerMVPS()
        {
            string[] split = _JSON.Split(new string[] { "\"mvps\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            return int.Parse(split[1].Split(',')[0]);
        }

        private int GetPlayerDeaths()
        {
            string[] split = _JSON.Split(new string[] { "\"deaths\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            return int.Parse(split[1].Split(',')[0]);
        }

        private int GetPlayerKills()
        {
            string[] split = _JSON.Split(new string[] { "\"kills\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            return int.Parse(split[1].Split(',')[0]);
        }

        private string GetPlayerName()
        {
            string splitStr = _JSON.Split(new string[] { "\"player\": {" }, StringSplitOptions.None)[1].Split('}')[0];
            string[] split = splitStr.Split(new string[] { "\"name\": \"" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;
            return split[1].Split('"')[0];
        }

        private bool HasPlayer()
        {
            return _JSON.Split(new string[] { "\"player\": {" }, StringSplitOptions.None).Length > 1;
        }

        public GameState(string JSON)
        {
            UpdateJson(JSON);
        }

        private BombState? GetRoundBombState()
        {
            string[] splitStrs = _JSON.Split(new string[] { "\"round\": {" }, StringSplitOptions.None);
            if (splitStrs.Length < 2)
                return null;
            string splitStr = splitStrs[1].Split('}')[0];
            string[] bombStates = splitStr.Split(new string[] { "\"bomb\": \"" }, StringSplitOptions.None);
            if (bombStates.Length < 2)
                return null;
            string bombState = bombStates[1].Split('"')[0];
            switch (bombState)
            {
                case "planted":
                    return BombState.Planted;
                case "exploded":
                    return BombState.Exploded;
                case "defused":
                    return BombState.Defused;
                default:
                    return null;
            }
        }

        private long GetTimeStamp()
        {
            string splitStr = _JSON.Split(new string[] { "\"provider\": {" }, StringSplitOptions.None)[1].Split('}')[0];
            string[] split = splitStr.Split(new string[] { "\"timestamp\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            return long.Parse(split[1].Trim());
        }

        private string GetMap()
        {
            string[] split = _JSON.Split(new string[] { "\"map\": {" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;
            string state = split[1].Split(new string[] { "\"name\": \"" }, StringSplitOptions.None)[1].Split('"')[0];
            return state;
        }

        private int GetTScore()
        {
            string[] split = _JSON.Split(new string[] { "\"map\": {" }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            string splitstr = split[1].Split(new string[] { "\"team_t\": {" }, StringSplitOptions.None)[1].Split('}')[0];
            return int.Parse(splitstr.Split(new string[] { "\"score\":" }, StringSplitOptions.None)[1].Split(',')[0]);
        }
        private int GetCTScore()
        {
            string[] split = _JSON.Split(new string[] { "\"map\": {" }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            string splitstr = split[1].Split(new string[] { "\"team_ct\": {" }, StringSplitOptions.None)[1].Split('}')[0];
            return int.Parse(splitstr.Split(new string[] { "\"score\":" }, StringSplitOptions.None)[1].Split(',')[0]);
        }
        private Mode? GetMode()
        {
            string[] split = _JSON.Split(new string[] { "\"map\": {" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;
            string state = split[1].Split(new string[] { "\"mode\": \"" }, StringSplitOptions.None)[1].Split('"')[0];
            switch (state)
            {
                case "competitive":
                    return Mode.Competitive;
                case "deathmatch":
                    return Mode.Deathmatch;
                case "casual":
                    return Mode.Casual;
                case "scrimcomp2v2":
                    return Mode.Wingman;
                case "gungameprogressive":
                    return Mode.ArmsRace;
                case "gungametrbomb":
                    return Mode.Demolition;
                case "coopmission":
                    return Mode.COOP;
                case "custom":
                    return Mode.Custom;
                case "survival":
                    return Mode.DangerZone;
                default:
                    return null;
            }
        }

        private string GetSteamID()
        {
            string splitStr = _JSON.Split(new string[] { "\"player\": {" }, StringSplitOptions.None)[1].Split('}')[0];
            string[] split = splitStr.Split(new string[] { "\"steamid\": \"" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;
            return split[1].Split('"')[0];
        }

        private string GetMySteamID()
        {
            string splitStr = _JSON.Split(new string[] { "\"provider\": {" }, StringSplitOptions.None)[1].Split('}')[0];
            string[] split = splitStr.Split(new string[] { "\"steamid\": \"" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;
            return split[1].Split('"')[0];
        }

        private bool HasDefuseKit()
        {
            string splitStr = _JSON.Split(new string[] { "\"player\": {" }, StringSplitOptions.None)[1].Split('}')[0];
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
            string[] split = _JSON.Split(new string[] { "\"bomb\": {" }, StringSplitOptions.None);
            if (split.Length < 2)
                return null;

            string state = split[1].Split(new string[] { "\"state\": \"" }, StringSplitOptions.None)[1];
            return state.Split('"')[0];
        }
        private bool GetHelmetState()
        {
            string[] split = _JSON.Split(new string[] { "\"helmet\": " }, StringSplitOptions.None);
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
            string[] split = _JSON.Split(new string[] { "\"money\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            return int.Parse(split[1].Split(',')[0]);
        }

        private int GetArmor()
        {
            string[] split = _JSON.Split(new string[] { "\"armor\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            int armor = int.Parse(split[1].Split(',')[0]);
            return armor;
        }

        private int GetHealth()
        {
            string[] split = _JSON.Split(new string[] { "\"health\": " }, StringSplitOptions.None);
            if (split.Length < 2)
                return -1;
            int health = int.Parse(split[1].Split(',')[0]);
            return health;
        }

        private Team? GetTeam()
        {
            string[] split = _JSON.Split(new string[] { "\"team\": \"" }, StringSplitOptions.None);
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
            string[] splitted = _JSON.Split(new string[] { "\"activity\": \"" }, StringSplitOptions.None);
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
            string[] splitted = _JSON.Split(new string[] { "\"round\": " }, StringSplitOptions.None);
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
            string[] split = _JSON.Split(new string[] { "\"round\": {" }, StringSplitOptions.None);
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
            string[] split = _JSON.Split(new string[] { "\"map\": {" }, StringSplitOptions.None);
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

        public void Dispose()
        {
            Timestamp = 0;
            MySteamID = null;
            _JSON = null;
            Player.Dispose();
            Player = null;
            Match.Dispose();
            Match = null;
            Round.Dispose();
            Round = null;
        }
    }
    public class Weapon : IDisposable
    {
        public int Index { get; internal set; }
        public int Bullets { get; internal set; }
        public int ClipSize { get; internal set; }
        public int ReserveBullets { get; internal set; }
        public string Name { get; internal set; }
        public WeaponType? Type { get; internal set; }
        public WeaponState? State { get; internal set; }

        public void Dispose()
        {
            Name = null;
            Type = null;
            State = null;
        }
    }
    public class Match : IDisposable
    {
        public Phase? Phase { get; internal set; }
        public Mode? Mode { get; internal set; }
        public string Map { get; internal set; }
        public int TScore { get; internal set; }
        public int CTScore { get; internal set; }

        public void Dispose()
        {
            Map = null;
            Phase = null;
            Mode = null;
        }
    }
    public class Round : IDisposable
    {
        public int CurrentRound { get; internal set; }
        public Phase? Phase { get; internal set; }
        public BombState? Bombstate { get; internal set; }

        public void Dispose()
        {
            Phase = null;
            Bombstate = null;
        }
    }
    public class Player : IDisposable
    {
        public Weapon ActiveWeapon { get; internal set; }
        public Weapon[] Weapons { get; internal set; }
        public Activity? CurrentActivity { get; internal set; }
        public Team? Team { get; internal set; }
        public int Health { get; internal set; }
        public int Armor { get; internal set; }
        public int Money { get; internal set; }
        public int Kills { get; internal set; }
        public int Deaths { get; internal set; }
        public int MVPS { get; internal set; }
        public bool HasHelmet { get; internal set; }
        public bool HasDefuseKit { get; internal set; }
        public string SteamID { get; internal set; }
        public string Name { get; internal set; }
        public bool HasWeapon(string name)
        {
            for (int i = 0; i < Weapons.Length; i++)
            {
                Weapon wep = Weapons[i];
                if (wep.Name == name)
                    return true;
            }
            return false;
        }
        public void Dispose()
        {
            ActiveWeapon.Dispose();
            ActiveWeapon = null;
            Weapons = null;
            CurrentActivity = null;
            Team = null;
            SteamID = null;
        }
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
                    string[] typeSplit = weapon.Split(new string[] { "\"type\": \"" }, StringSplitOptions.None);
                    string type = typeSplit.Length > 1 ? typeSplit[1].Split('"')[0] : "None";
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
                catch
                {
                    return new Weapon()
                    {
                        Index = index,
                        Name = "NULL",
                        Type = null,
                        State = null,
                        Bullets = -1,
                        ReserveBullets = -1,
                        ClipSize = -1
                    };
                }
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
                case "Grenade":
                    return WeaponType.Grenade;
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
