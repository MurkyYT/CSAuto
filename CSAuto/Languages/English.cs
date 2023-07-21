﻿using System.Collections.Generic;

namespace CSAuto.Languages
{
    class English
    {
        static Dictionary<string, string> translation = new Dictionary<string, string>()
        {
            ["language_english"] = "English",
            ["language_russian"] = "Russian",
            ["menu_debug"] = "Debug",
            ["menu_language"] = "Language",
            ["menu_mobile"] = "Mobile App",
            ["menu_exit"] = "Exit",
            ["menu_automation"] = "Automation",
            ["menu_options"] = "Options",
            ["menu_opendebug"] = "Open Debug Window",
            ["menu_checkforupdates"] = "Check for updates",
            ["menu_enterip"] = "Enter IP address",
            ["menu_discordrpc"] = "Discord RPC",
            ["menu_enabled"] = "Enabled",
            ["menu_autocheckupdates"] = "Check For Updates On Startup",
            ["menu_autospotify"] = "Auto Pause/Resume Spotify",
            ["menu_startup"] = "Start With Windows",
            ["menu_continuespray"] = "Continue Spraying (Experimental)",
            ["menu_savedebugframes"] = "Save Frames",
            ["menu_savedebuglogs"] = "Save Logs",
            ["menu_autoaccept"] = "Auto Accept Match",
            ["menu_autobuyarmor"] = "Auto Buy Armor",
            ["menu_autobuydefuse"] = "Auto Buy Defuse Kit",
            ["menu_preferarmor"] = "Prefer armor",
            ["menu_autobuy"] = "Auto Buy",
            ["menu_discord"] = "Discord",
            ["menu_autoreload"] = "Auto Reload",
            ["title_debugwind"] = "CSAuto - Debug Window",
            ["title_update"] = "Check for updates (CSAuto)",
            ["title_restartneeded"] = "Restart app",
            ["title_error"] = "Error",
            ["inputtitle_mobileip"] = "Mobile Phone IP Address",
            ["inputtext_mobileip"] = "Enter the IP address you see in the app:",
            ["msgbox_latestversion"] = "You have the latest version!",
            ["msgbox_newerversion1"] = "Found newer verison",
            ["msgbox_newerversion2"] = "would you like to download it?",
            ["msgbox_restartneeded"] = "You must restart the program to apply these changes",
            ["error_update"] = "Couldn't check for updates. Try again later",
            ["error_startup1"] = "An error ocurred",
            ["error_startup2"] = "Try to download the latest version from github.",
            ["menu_notifications"] = "Notifications",
            ["menu_acceptednotification"] = "Accepted match",
            ["menu_mapnotification"] = "Loaded on map",
            ["menu_lobbynotification"] = "Loaded in lobby",
            ["menu_connectednotification"] = "Computer connected",
            ["menu_crashednotification"] = "Game crashed",
            ["server_computer"] = "Computer",
            ["server_online"] = "is online",
            ["server_loadedmap"] = "Loaded on map",
            ["server_mode"] = "in mode",
            ["server_loadedlobby"] = "Loaded in lobby!",
            ["server_gamecrash"] = "The game crashed!",
            ["server_acceptmatch"] = "Accepted a match!",
            ["menu_bombnotification"] = "Bomb information",
            ["server_timeleft"] = "Bomb seconds left:",
            ["server_bombexplode"] = "The bomb exploded",
            ["server_bombdefuse"] = "Bomb has been defused",
            ["exception_steamnotfound"] = "Couldn't find Steam Path",
            ["exception_nonetworkadapter"] = "No network adapters with an IPv4 address in the system!",
            ["exception_csgonotfound"] = "Couldn't find CS:GO directory",
            ["menu_entersteamkey"] = "Enter Steam Web API Key",
            ["inputtitle_steamkey"] = "Steam Web API Key",
            ["inputtext_steamkey"] = "Please enter your Steam Web API Key",
            ["menu_lobbycount"] = "Show players lobby count",
        };
        public static string Get(string category)
        {
            if (translation.ContainsKey(category)) return translation[category]; else return category;
        }
    }
}