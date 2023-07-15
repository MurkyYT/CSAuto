using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAuto
{
    public static class AppLanguage
    {
        public static string Get(string category)
        {
            try
            {
                switch (Properties.Settings.Default.currentLanguage)
                {
                    case "language_english":
                        return English[category];
                    case "language_russian":
                        return Russian[category];
                }
            }
            catch { }
            return category;
        }
        static readonly Dictionary<string, string> English = new Dictionary<string, string>()
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
        };
        static readonly Dictionary<string, string> Russian = new Dictionary<string, string>()
        {
            ["language_english"] = "Английский",
            ["language_russian"] = "Русский",
            ["menu_debug"] = "Отладка",
            ["menu_language"] = "Язык",
            ["menu_mobile"] = "Мобильный Компаньон",
            ["menu_exit"] = "Выход",
            ["menu_automation"] = "Автоматизация",
            ["menu_options"] = "Настройки",
            ["menu_opendebug"] = "Открыть окно отладки",
            ["menu_checkforupdates"] = "Проверить обновления",
            ["menu_enterip"] = "Ввести IP-адрес",
            ["menu_discordrpc"] = "Discord RPC",
            ["menu_enabled"] = "Включить",
            ["menu_autocheckupdates"] = "Проверять обновления автоматически",
            ["menu_autospotify"] = "Автоматическая пауза/возобновление Spotify",
            ["menu_startup"] = "Автозапуск",
            ["menu_continuespray"] = "Продолжить стрелять (экспериментальное)",
            ["menu_savedebugframes"] = "Сохранение кадров",
            ["menu_savedebuglogs"] = "Сохранение логов",
            ["menu_autoaccept"] = "Автоматическое подтверждение игры",
            ["menu_autobuyarmor"] = "Автоматическая покупка брони",
            ["menu_autobuydefuse"] = "Автоматическая покупка дефузов",
            ["menu_preferarmor"] = "Предпочитать броню",
            ["menu_autobuy"] = "Автоматическая покупка",
            ["menu_discord"] = "Discord",
            ["menu_autoreload"] = "Автоматическая перезарядка",
            ["title_debugwind"] = "CSAuto - Окно Отладки",
            ["title_update"] = "Проверка обновлений (CSAuto)",
            ["title_error"] = "Ошибка",
            ["title_restartneeded"] = "Перезапустить программу",
            ["inputtitle_mobileip"] = "IP-адрес телефона",
            ["inputtext_mobileip"] = "Введите IP-адрес, который видите в приложение:",
            ["msgbox_latestversion"] = "У вас последняя версия!",
            ["msgbox_newerversion1"] = "Найдена новая версия",
            ["msgbox_newerversion2"] = "Хотите скачать её?",
            ["msgbox_restartneeded"] = "Чтобы изменения вступили в силу, нужно перезапустить программу",
            ["error_update"] = "Не получилось проверить обновления. Попробуйте позже",
            ["error_startup1"] = "Случилась ошибка",
            ["error_startup2"] = "Попытайтесь скачать последнюю версию с GitHub.",
        };
    }
}
