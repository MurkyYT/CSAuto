<h1 align="center">CSAuto</h1>
<!-- 
<p align="center">
   <a href="https://www.virustotal.com/gui/file/9297cb519b209e6f0c7d937c93cea40aefda3f9d1b870c74405097b3deb0596b/detection"><img src="https://github.com/MurkyYT/CSAuto/blob/dev/virustotal_icon.png?raw=true" height="40" alt="VirusTotal scan"></a>
</p>
-->
<p align="center">
  <img width="auto" src="https://img.shields.io/github/release-date/murkyyt/csauto?label=Latest%20release" alt="Latest Release">
  <img width="auto" src="https://img.shields.io/github/v/tag/murkyyt/csauto?label=Latest%20version" alt="Latest Version">
  <img width="auto" src="https://img.shields.io/github/downloads/murkyyt/csauto/total?color=brightgreen&label=Total%20downloads" alt="Total Downloads">
</p>
<p align="center">
  <a href="https://github.com/MurkyYT/CSAuto/blob/master/README.md"><img src="https://img.shields.io/badge/lang-en-red.svg"></a>
  <a href="https://github.com/MurkyYT/CSAuto/blob/master/README_ru.md"><img src="https://img.shields.io/badge/lang-ru-yellow.svg"></a>
</p>
<h1 align="center">Contributors</h1>
<p align="center">    
  <a href="https://github.com/NoPlagiarism">NoPlagiarism</a>
  <!-- more links here -->
</p>

<h1 align="center">Description</h1>
Have you ever started searching for a game in CS2 went for a bit, and see that you missed the accept button?  
frustrating isn't it?  
Or have you ever forgot to buy armor or defuse kit as ct?  
  
*Dont Worry!*  
**CSAuto** is the software for you, CSAuto can:
* Auto accept matches for you
* ~~Auto reload for you when you have 0 bullets left and keep spraying after reloading! (continue spraying might lag out trying to find a fix)~~ (GSI might not be enabled in cs2)
* ~~Auto buy armor for you or auto refill it when you have less then 70 left!~~ (GSI might not be enabled in cs2)
* ~~Auto buy defuse kit for you as a CT~~ (GSI might not be enabled in cs2)
* ~~Auto pause/resume spotify song~~ (GSI might not be enabled in cs2)
* ~~Show status about cs, when you are in lobby it shows your friendcode and amount of players in lobby, when in match it shows the map, mode, score, and round phase (amount of players in lobby is disabled by default, you will have to enable it manually, it is like that because i dont know if its bannable or not)~~ (GSI might not be enabled in cs2)
* ~~Send time left till bomb exploded to mobile! (not accurate as of right now)~~ (GSI might not be enabled in cs2)

**Image demonstration of the menu:**  
![right-click-menu](menuimage.png)
## Installation
<p align="center">    
  <a href="https://github.com/murkyyt/csauto/releases"><img src="https://github.com/machiav3lli/oandbackupx/blob/034b226cea5c1b30eb4f6a6f313e4dadcbb0ece4/badge_github.png" height="80" alt="Get On Github"></a>
</p>

## FAQ
### **How to connect to the mobile app**
  1. Make sure you have installed the app on your phone
  2. Make sure you are connected to the same network
  3. In the mobile app start the server (and allow ignoring battery optimization)
  4. Go to the desktop app, in the 'Mobile App' menu and make sure it is enabled
  5. Go again to the 'Mobile App' menu and press 'Enter IP address'
  6. In the input box that appeared, enter the IP address you see in the notification on the phone
  7. After that you should get a notification which says 'Computer {MACHINE-NAME} (PC-IP) is online' 
### The app couldn't set the launch options, what to do?
  1. Open steam library
  2. Right click on CS2 and press properties
  3. In the general tab you have launch options at the bottom
  4. Add -gamestateintegration to the launch options
  5. Close and start the game
### **Discord doesn't show amount of players in lobby**
  1. Open the discord sub-menu
  2. Press enter 'Enter Steam Web API Key'
  3. Get your steam web api key from [here](https://steamcommunity.com/dev)
  4. After you entered it you should have it
  5. If you still dont have it, make sure you created a lobby by inviting someone
### **How to get Telegram Chat id**
  1. Open [Raw Data Bot](https://t.me/raw_info_bot)
  2. Send a message
  3. Copy your 'Chat ID'
  4. Enter it in the desktop app
  5. After that you should get a notification which says 'Computer {MACHINE-NAME} (PC-IP) is online'
## Suggestions
*If you have any suggestions you can create an issue/discussion with the suggestion in it*  
**Thanks in advance :)**
## Development

### Building the app

1. Install Visual Studio 2022 with C# and Xamarin.
2. Install Inno Setup Compiler.
3. Clone the repository and open the solution in Visual Studio 2022
4. Build the app
5. If you want the installer you can run the compile.bat
