using System.Collections.Generic;

namespace CSAuto.Languages
{
    class Russian
    {
        static Dictionary<string, string> translation = new Dictionary<string, string>()
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
            ["menu_notifications"] = "Уведомления",
            ["menu_acceptednotification"] = "Принял матч",
            ["menu_mapnotification"] = "Загрузился на карте",
            ["menu_lobbynotification"] = "Загрузился в главном меню",
            ["menu_connectednotification"] = "Компьютер подключился",
            ["menu_crashednotification"] = "Игра вылетела",
            ["server_computer"] = "Компьютер",
            ["server_online"] = "в сети",
            ["server_loadedmap"] = "Загрузился на карте",
            ["server_mode"] = "в режиме",
            ["server_loadedlobby"] = "Загрузился в главном меню!",
            ["server_gamecrash"] = "Игра вылетела!",
            ["server_acceptmatch"] = "Принял матч!",
            ["menu_bombnotification"] = "Информация о бомбы",
            ["server_timeleft"] = "До взрыва осталось: ",
            ["server_bombexplode"] = "Бомба взорвалась",
            ["server_bombdefuse"] = "Бомба обезврежена",
            ["exception_steamnotfound"] = "Не удалось найти путь к Steam",
            ["exception_nonetworkadapter"] = "В системе нет сетевых адаптеров с IPv4-адресом!",
            ["exception_csgonotfound"] = "Не удалось найти папку CS:GO",
            ["menu_entersteamkey"] = "Ввести ключ Steam Web API",
            ["inputtitle_steamkey"] = "Ключ Steam Web API",
            ["inputtext_steamkey"] = "Пожалуйста, введите свой ключ Steam Web API",
            ["menu_lobbycount"] = "Показать количество игроков в лобби",
        };
        public static string Get(string category)
        {
            if (translation.ContainsKey(category)) return translation[category]; else return English.Get(category);
        }
    }
}
