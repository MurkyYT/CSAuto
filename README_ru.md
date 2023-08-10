<h1 align="center">CSAuto</h1>
<!--
<p align="center">
   <a href="https://www.virustotal.com/gui/file/f68ba52499a4158e2d72876c33ea8ee5ade3ab496b2da4bbcb109383a29a61ed?nocache=1"><img src="https://github.com/MurkyYT/CSAuto/blob/dev/virustotal_icon.png?raw=true" height="40" alt="VirusTotal scan"></a>
</p>
-->
<p align="center">
  <img width="auto" src="https://img.shields.io/github/release-date/murkyyt/csauto?label=Latest%20release" alt="Latest Release">
  <img width="auto" src="https://img.shields.io/github/v/tag/murkyyt/csauto?label=Latest%20version" alt="Latest Version">
  <img width="auto" src="https://img.shields.io/github/downloads/murkyyt/csauto/total?color=brightgreen&label=Total%20downloads" alt="Total Downloads">
</p>
<p align="center">
  <a href="https://github.com/MurkyYT/CSAuto/blob/master/README.md"><img src="https://img.shields.io/badge/язык-en-red.svg"></a>
  <a href="https://github.com/MurkyYT/CSAuto/blob/master/README_ru.md"><img src="https://img.shields.io/badge/язык-ru-yellow.svg"></a>
</p>
<h1 align="center">Участники</h1>
<p align="center">    
  <a href="https://github.com/NoPlagiarism">NoPlagiarism</a>
  <!-- more links here -->
</p>

<h1 align="center">Описание</h1>
Вы когда нибудь начали искать игру в CS2 ушли на немного, и видите что вы не приняли игру?
неприятно, не правда ли?
Или вы забыли купить броню или дефуза за кт?

*Не волнуйтесь!*  
**CSAuto** это программа для вас, CSAuto может:
* Принимать матчи для вас
* ~~Автоматически перезарядить ваше оружие когда осталось 0 патронов и продолжать стрелять дальше! (продолжать стрелять может залагат иногда пытаюсь найти фикс)~~ (GSI может не быть в cs2)
* ~~Автоматически купить броню для вас или восстановить её когда у вас меньше 70 осталось!~~ (GSI может не быть в cs2)
* ~~Автоматически покупать дефуза для вас за кт~~ (GSI может не быть в cs2) 
* ~~Автоматическая пауза/ возобновление песни spotify~~ (GSI может не быть в cs2)
* ~~Показывать статус cs, когда вы находитесь в лобби, отображается код друга и количество игроков в лобби, когда в матче отображается карта, режим, счет и фаза раунда (количество игроков в лобби по умолчанию отключено, вам придется включить его вручную, это так, потому что я не знаю, можно ли получить за него бан)~~ (GSI может не быть в cs2)
* ~~Отправлять время, оставшееся до взрыва бомбы, на мобильный телефон! (не точно на данный момент)~~ (GSI может не быть в cs2)

**Фотография меню:**  
![right-click-menu](menuimage.png)
## Установка
<p align="center">    
  <a href="https://github.com/murkyyt/csauto/releases"><img src="https://github.com/machiav3lli/oandbackupx/blob/034b226cea5c1b30eb4f6a6f313e4dadcbb0ece4/badge_github.png" height="80" alt="Get On Github"></a>
</p>

## FAQ
### **Как подключиться к мобильному приложению**
  1. Убедитесь, что вы установили приложение на свой телефон
  2. Убедитесь, что вы подключены к той же сети
  3. В мобильном приложении запустите сервер (и разрешите игнорировать оптимизацию заряда батареи)
  4. Перейдите в настольное приложение в меню "Мобильный Компаньон" и убедитесь, что он включен
  5. Снова перейдите в меню "Мобильный Компаньон" и нажмите "Ввести IP-адрес"
  6. В появившемся поле ввода введите IP-адрес, который вы видите в уведомлении на телефоне
  7. После этого вы должны получить уведомление, в котором говорится: 'Computer {MACHINE-NAME} (PC-IP) is online'
### Приложению не удалось настроить параметры запуска, что делать?
  1. Откройте библиотеку steam
  2. Щелкните правой кнопкой мыши на CS2 и нажмите свойства
  3. На вкладке Общие внизу у вас есть параметры запуска
  4. Добавьте -gamestateintegration в параметры запуска
  5. Закройте и запустите игру
### **Discord не показывает количество игроков в лобби**
   1. Откройте меню discord
   2. Нажмите "Ввесте ключ Steam Web API".
   3. Получите свой ключ steam web api [здесь](https://steamcommunity.com/dev)
   4. После того, как вы ввели его, у вас должно работать
   5. Если у вас все еще его нет, убедитесь, что вы создали лобби, пригласив кого-нибудь
### **Как получать уведомления в Telegram**
   1. Отправьте сообщение на [бот](https://t.me/csautonotification_bot)
   2. Получите свой идентификатор чата, отправив сообщение [этому боту](https://t.me/raw_info_bot)
   3. Скопируйте свой "Идентификатор чата"
   4. Введите его в приложении
   5. После этого вы должны получить уведомление, в котором говорится: 'Computer {MACHINE-NAME} (PC-IP) is online'
## Предложения
*Если у вас есть какие-либо предложения, вы можете создать проблему / обсуждение с включенным в него предложением*
**Заранее спасибо:)**
## Разработка

### Скомпилировать приложения

1. Установите Visual Studio 2022 с C# и Xamarin.
2. Установите Inno Setup Compiler.
3. Клонируйте репозиторий и откройте решение в Visual Studio 2022
4. Скомпилируете приложения
5. Если вам нужен установщик, вы можете запустить файл compile.bat
