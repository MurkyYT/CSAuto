<div align="center">
    <a href="https://csauto.vercel.app"><img width=150 src="https://raw.githubusercontent.com/MurkyYT/CSAuto/master/Docs/logo/CSAuto_logo.svg" alt="logo"/></a>

   <h1>CSAuto</h1>
   
  <a href="https://discord.gg/57ZEVZgm5W"><img src="https://dcbadge.vercel.app/api/server/57ZEVZgm5W"></a>
</p>
<p>
  <img width="auto" src="https://img.shields.io/github/release-date/murkyyt/csauto?label=%D0%9F%D0%BE%D1%81%D0%BB%D0%B5%D0%B4%D0%BD%D0%B8%D0%B9%20%D1%80%D0%B5%D0%BB%D0%B8%D0%B7&style=for-the-badge" alt="Latest Release">
  <img width="auto" src="https://img.shields.io/github/v/tag/murkyyt/csauto?label=%D0%9F%D0%BE%D1%81%D0%BB%D0%B5%D0%B4%D0%BD%D0%B0%D1%8F%20%D0%B2%D0%B5%D1%80%D1%81%D0%B8%D1%8F&style=for-the-badge" alt="Latest Version">
  <img width="auto" src="https://img.shields.io/github/downloads/murkyyt/csauto/total?color=brightgreen&label=%D0%BA%D0%BE%D0%BB%D0%B8%D1%87%D0%B5%D1%82%D1%81%D0%B2%D0%BE%20%D1%81%D0%BA%D0%B0%D1%87%D0%B5%D0%BA&style=for-the-badge" alt="Total Downloads">
</p>
<p>
  <a href="https://learn.microsoft.com/en-us/dotnet/csharp"><img width="auto" src="https://img.shields.io/github/languages/top/murkyyt/csauto?logo=csharp&logoColor=green&style=for-the-badge" alt="Language"></a>
  <a href="https://en.wikipedia.org/wiki/BSD_licenses"><img width="auto" src="https://img.shields.io/github/license/murkyyt/csauto?style=for-the-badge&label=%D0%BB%D0%B8%D1%86%D0%B5%D0%BD%D0%B7%D0%B8%D1%8F" alt="License"></a>
</p>
<p>
  <a href="https://github.com/MurkyYT/CSAuto/blob/master/README.md"><img src="https://img.shields.io/badge/язык-англ-red.svg?style=for-the-badge"></a>
  <a href="https://github.com/MurkyYT/CSAuto/blob/master/Docs/README_ru.md"><img src="https://img.shields.io/badge/язык-рус-yellow.svg?style=for-the-badge"></a>
</p>
</div>

<h1 align="center">Описание</h1>

Вы когда нибудь начинали искать игру в CS2, уходили ненадолго, а возвращаясь видите что не приняли игру?
Неприятно, не правда ли? Или забыли купить броню или дефуза за кт?

*Не волнуйтесь!*

**CSAuto** это программа для вас

## Возможности

* Принимает матчи за вас
* Автоматически перезаряжает ваше оружие когда осталось 0 патронов и продолжает стрелять дальше! (продолжение стрельбы может залагать иногда пытаюсь найти фикс)
* Автоматическая покупка выбранных предметов в настраиваемом меню!
* Автоматическая пауза/возобновление в Spotify
* Показывает настраиваемую информацию об игре в Discord Rich Presence (Могут забанить за отображение количества игроков в лобби!)
* Отправлять время, оставшееся до взрыва бомбы, на мобильный телефон! (не точно на данный момент)
* Сфокусириваться на игре, когда появится кнопка "Принять", и возвращаться к предыдущему окну!

## Скрины

![right-click-menu](assets/menuimage.png)
![discord-menu](assets/app_discord.png)

## Установка

<p align="center">
<a href="https://github.com/murkyyt/csauto/releases/latest/download/CSAuto_Portable.zip"><img src="assets/windows-portable-badge.png" height ="80" alt="Get On Windows (Portable)"></a>
<a href="https://github.com/murkyyt/csauto/releases/latest/download/CSAuto_Android.apk"><img src="assets/android-badge.png" height ="80" alt="Get On Android"></a>
<a href="https://github.com/murkyyt/csauto/releases/latest/download/CSAuto_Installer.exe"><img src="assets/windows-installer-badge.png" height ="80" alt="Get On Windows (Installer)"></a>
</p>

## Параметры запуска
  * `--show` - Показывать окно настроек при запуске приложения
  * `--maximized` - Сделать так, чтобы окно настроек всегда открывалось развернутым
  * `--language [название языка]` - Изменить язык приложения на указанный (например en,ru), настройки не изменяются
  * `--portable` - Запуск приложения в портативном режиме
  * `--log` - Включение ведения журнала, независимо от того, включено ли оно в настройках, не изменяет настройки
  * `--cs` - Запустить кс вместе с CSAuto

## FAQ

### Как подключиться через мобильное приложение

  1. Убедитесь, что приложение установлено на вашем телефоне
  2. Убедитесь, что вы подключены к той же сети, что и компьютер
  3. В настольном приложении перейдите в категорию `Сервер` и запомните `IP` и `Порт`
  4. Перейдите в настольное приложение в категории `Настройки -> Уведомления по телефону` и убедитесь, что оно включено
  5. Перейдите в мобильное приложение и введите `IP` и `Порт`, которые вы видели в категории `Сервер"
  6. Запустите cs и попробуйте подключиться к серверу в мобильном приложении

### Приложению не удалось настроить параметры запуска, что делать?

  1. Откройте библиотеку Steam
  2. Щелкните правой кнопкой мыши на CS2 и нажмите свойства
  3. На вкладке Общие внизу у вас есть параметры запуска
  4. Добавьте `-gamestateintegration` в параметры запуска
  5. Закройте и запустите игру

### Discord не показывает количество игроков в лобби

   1. Откройте категорию Discord
   2. Получите свой ключ Steam Web API [здесь](https://steamcommunity.com/dev)
   3. Введите свой ключ Steam Web API
   4. После того, как вы ввели его, у вас должно сработать
   5. Если у вас все еще его нет, убедитесь, что вы создали лобби, пригласив кого-нибудь, и то что вы включили показывать количество игроков

### Как получать уведомления в Telegram

   1. Отправьте сообщение [боту](https://t.me/csautonotification_bot)
   2. Получите свой идентификатор чата, отправив сообщение [этому боту](https://t.me/raw_info_bot)
   3. Скопируйте свой "Идентификатор чата"
   4. Перейдите в категорию "Уведомления по телефону" и введите полученный вами идентификатор чата
   
### Какая самая последняя версия для CS:GO
	
  Последняя версия, выпущенная для CS:GO 1.1.2, которую можно скачать [здесь](https://github.com/MurkyYT/CSAuto/releases/tag/1.1.2) Имейте в виду, что некоторые функции могут работать не так, как предполагалось

## Предложения

*Если у вас есть какие-либо предложения, то вы можете создать проблему / обсуждение с включенным в него предложением или использовать [Discord сервер](https://discord.gg/57ZEVZgm5W)*

**Заранее спасибо:)**

## Участники

<a href="https://github.com/murkyyt/csauto/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=murkyyt/csauto" />
</a>

## Разработка

### Скомпилировать приложения

1. Установите Visual Studio 2022 с C# и .NET MAUI (возможно, вам также понадобится Xamarin).
2. Установите Inno Setup Compiler.
3. Клонируйте репозиторий и откройте решение в Visual Studio 2022
4. Скомпилируете приложения
5. Если вам нужен установщик, вы можете запустить файл compile.bat

## Дисклеймер

CSAuto не является аффилированным лицом, не одобрено и никоим образом официально не связано с [Valve](https://www.valvesoftware.com/en/) и/или [Counter-Strike](https://www.counter-strike.net/)
