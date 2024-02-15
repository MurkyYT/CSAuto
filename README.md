<div align="center">
    <a href="https://csauto.vercel.app"><img width=150 src="https://raw.githubusercontent.com/MurkyYT/CSAuto/master/Docs/logo/CSAuto_logo.svg" alt="logo"/></a>

   <h1>CSAuto</h1>

  <a href="https://discord.gg/57ZEVZgm5W"><img src="https://dcbadge.vercel.app/api/server/57ZEVZgm5W"></a>
</p>
<p>
  <img width="auto" src="https://img.shields.io/github/release-date/murkyyt/csauto?label=Latest%20release&style=for-the-badge" alt="Latest Release">
  <img width="auto" src="https://img.shields.io/github/v/tag/murkyyt/csauto?label=Latest%20version&style=for-the-badge" alt="Latest Version">
  <img width="auto" src="https://img.shields.io/github/downloads/murkyyt/csauto/total?color=brightgreen&label=Total%20downloads&style=for-the-badge" alt="Total Downloads">
</p>
<p>
  <a href="https://github.com/MurkyYT/CSAuto/blob/master/README.md"><img src="https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge"></a>
  <a href="https://github.com/MurkyYT/CSAuto/blob/master/Docs/README_ru.md"><img src="https://img.shields.io/badge/lang-ru-yellow.svg?style=for-the-badge"></a>
</p>
</div>

## Description

Have you ever started searching for a game in CS2, left a keyboard for a bit, and noticed that you missed the accept button?
frustrating isn't it?
Or have you ever forgotten to buy armor or defuse kit as ct?
  
*Don't Worry!*  
**CSAuto** is the software for you

## Features

* Auto accept matches for you
* Auto reload for you when you have 0 bullets left and keep spraying after reloading! (continue spraying might lag out trying to find a fix)
* Auto buy selected items in customizable menu!
* Auto pause/resume spotify song
* Show customizable game info in Discord Rich Presence (show amount of players in lobby can be bannable!)
* Send time left till bomb exploded to mobile! (not accurate as of right now)

## Screenshots

![right-click-menu](Docs/assets/menuimage.png)
![discord-menu](Docs/assets/app_discord.png)

## Installation

<p align="center">    
<a href="https://github.com/murkyyt/csauto/releases/latest/download/CSAuto_Portable.zip"><img src="Docs/assets/windows-portable-badge.png" height ="80" alt="Get On Windows (Portable)"></a>
<a href="https://github.com/murkyyt/csauto/releases/latest/download/CSAuto_Android.apk"><img src="Docs/assets/android-badge.png" height ="80" alt="Get On Android"></a>
<a href="https://github.com/murkyyt/csauto/releases/latest/download/CSAuto_Installer.exe"><img src="Docs/assets/windows-installer-badge.png" height ="80" alt="Get On Windows (Installer)"></a>
</p>

## FAQ

### How to connect to the mobile app

  1. Make sure you have installed the app on your phone
  2. Make sure you are connected to the same network
  3. In the mobile app start the server (and allow ignoring battery optimization)
  4. Go to the desktop app, in the 'Phone notifications' category and make sure it is enabled
  5. Go again to the 'Phone notifications' category and enter the IP address you see in the notification on the phone

### The app couldn't set the launch options, what to do?

  1. Open Steam library
  2. Right-click on CS2 and press properties
  3. In the general tab you have launch options at the bottom
  4. Add `-gamestateintegration` to the launch options
  5. Close and start the game

### Discord doesn't show amount of players in lobby

  1. Open the Discord category
  2. Get your Steam web api key from [here](https://steamcommunity.com/dev)
  3. Enter your Steam Web API Key
  4. After you entered it you should have it
  5. If you still don't have it, make sure you created a lobby by inviting someone and enabled it

### How to get notifications in Telegram

   1. Send a message to the [bot](https://t.me/csautonotification_bot)
   2. Get your chat id by sending a message to [this bot](https://t.me/raw_info_bot)
   3. Copy your 'Chat ID'
   4. Go to the 'Phone notifications' category and enter the Chat ID you got

## Suggestions

*If you have any suggestions you can create an issue/discussion with the suggestion in it or use [Discord server](https://discord.gg/57ZEVZgm5W)*

**Thanks in advance :)**

## Contributors

| Name | Helped with |
| ---- | ----------- |
| ![NoPlagiarism's pfp](https://github.com/NoPlagiarism.png?size=30) [NoPlagiarsm](https://github.com/NoPlagiarism) | Translation, design and app ideas |

## Development

### Building the app

1. Install Visual Studio 2022 with C# and Xamarin.
2. Install Inno Setup Compiler.
3. Clone the repository and open the solution in Visual Studio 2022
4. Build the app
5. If you want the installer you can run the compile.bat
