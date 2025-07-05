### Version 2.2.0 changelog
- Added Hebrew translation
- Added support for RTL languages
- Fixed notifications not being translated
- Little UI fix ups
- Fix with crash reports

---

### Version 2.1.10 changelog
- Hot fix for telegram messages

---

### Version 2.1.9 changelog
- Added yes/no button to send crash report or not

---

### Version 2.1.8 changelog
- Hot fix for en-EN locale

---

Version 2.1.7 changelog
- Added automatic crash report sending via telegram

---

### Version 2.1.6 changelog
- Auto buy now doesnt open the buy menu
- Fixed crash associated with cs proccess

---

### Version 2.1.5 changelog
- Added support for all music players
- Spotify can be in the tray when auto pause/resume feature is enabled
- Fixed mobile app android 14 compatibility
- Little updater fixes
- Added searching all folders where cs:go presumably is
- Little fixes with screenshotting
- Small fix with steamapi exe path
- Fixed auto reload spraying bugging out using a workaround to send commands
- Fixed DXGI capture crashing when cs window is not on primary desktop

---

### Version 2.1.4 changelog
- Added a slight delay when auto accepting to hopefully eliminate not clicking in the right place
- Auto accept now always checks for accept button even if accepted, this way the app shouldn't miss the accept button at all
- Reordered categories placements from most useful to less useful
- Cleaned up debug ui
- Moved relevant debug info to a seperate window with a copyable text box
- Added about menu with copyright notice
- Added auto optimization on startup for HDR displays
- Fixed spotify auto pause/resume crashing (thanks @nevespt for pointing out that there is a problem when spotify window is hidden)
- Added a clarification about spotify auto pause/resume
- Fixed portable settings not always saved
- Adjusted M4A4's and FAMAS's price according to [premier season 2 update](https://steamcommunity.com/games/CSGO/announcements/detail/520830071182721027?utm_source=SteamDB)

---

### Version 2.1.3 changelog
- Fixed spamming player is in lobby notification
- Added an option to disable auto buy rebuying after refunding
- Made updater window non-resizable
- Context menu about menu item look was enhanced
- Fixed parsing of map icons

---

### Version 2.1.2a changelog
- Fixed Discord Activity being stuck in last match when player is already in lobby
- Fix with server thread when closing cs which caused a crash

---

### Version 2.1.2 changelog
- Right click menu title is now in the same color as the accent color
- No need to restart the application when changing from dark to light theme and vice versa
- Added icons to each category in settings window
- Added separator between the menu and the ui
- Fixed a very rare crash which occurred on cs exit
- Refactored the communication protocol between the mobile app and the desktop app
- Revamped the entire mobile app, if you use it update or it wont work!
- Small fix with loading map icons
- Changed applications folder structure
- Added rounded corners to context menu when using Windows 11
- Fixed a crash where SteamID3 can't be found at all
- Fixed log loading not working when SteamID3 can't be found
- Fixed a very rare crash
- Made mouse input more reliable
- The server is now the desktop app, follow the [FAQ](https://github.com/MurkyYT/CSAuto/tree/master?tab=readme-ov-file#faq) to see how to set it up
- Added newer mouse input technique, you can still enable the old one if the new one doesn't work
- Little startup optimizations

---

### Version 2.1.1 changelog
- Added global toggle for auto buy
- Added more in depth debug settings in the debug category (they are resetted every startup)
- Added newer style to default message boxes
- Corrected `M4A4` and `Incendiary Grenade` prices according to the [24.5.24 update](https://steamcommunity.com/games/CSGO/announcements/detail/4177730135016140041)

---

### Version 2.1.0 changelog
- Changed the title, it is now centered and shows `CSAuto` in bold with the version
- Added new launch option, `--log`, enabling logging, regardless of whether it is enabled in the settings, doesn't change the settings
- Small fix with spotify process finding
- Changes to russian translation (`Загрузить настройки` -> `Экспортировать настройки`)
- Settings importing now shows the gui window only if it was open before
- Languages now interpolates '\n' correctly
- Languages are now stored as resources, fixes gui window not always showing text
- Fixed a bug when entering debug category, log will always scroll down
- Added new automation feature, `CSAuto` can try to focus on cs window when game is found, only if enabled in settings
- If `CSAuto` focused on cs window it can focus back to the original window
- New launch option, `--cs`, launches CS together with `CSAuto`
- `CSAuto` now tries to load workshop maps icons as well
- `--language` now uses two letters to represent languages, e.g `en` or `ru`

---

### Version 2.0.9 changelog
- All Discord settings are now in the same place
- Fix with settings import/export message boxes being shown when not needed
- Settings import/export now utilizes utf8 encoding which should fix all bugs with characters disappearing 
- Portable version now saves config inside the app folder!
- Added `--portable` launch option, if you installed the app without using the portable zip then you can use it to run in portable mode
- Added alternative links to changelog in the changelog category
- Added custom telegram bot token option
- Auto accept now works with `Windowed` `Fullscreen` and `Windowed Fullscreen` modes
- Reduced size of the mobile app
- Optimizations in the mobile app
- Added `updater.exe` that will update the app for you without you having to download it manually
- Sped up application startup time

## What's Changed
* Changelog: Preparation for new website by @NoPlagiarism in https://github.com/MurkyYT/CSAuto/pull/27
* Workflow: Optional version update by @NoPlagiarism in https://github.com/MurkyYT/CSAuto/pull/28
* README: improvements, some refactor, new images by @NoPlagiarism in https://github.com/MurkyYT/CSAuto/pull/29

---

### Version 2.0.8 changelog
- Markdown parser updated, changelog now looks a bit different
- Notify icon text now shows only `CSAuto`
- Added support for rounded corners in settings window *(right click menu with rounded corners is broken)*
- Fixed mobile app crashing when getting `GSI` info
- Fix `GSI` parsing bug when weapon has no type
- You can import and export settings
- New Auto Buy menu!
  - The new `auto buy menu` looks like the actual `buy menu`
  - You can enable as of right now auto buying of `Kevlar` `Kevlar & Helmet` `Zeus` `Decoy` `Molotov\Incendiary` `HE Grenade` `Flashbang` `Smoke` and `Defuse Kit`
  - You can arrange custom order of weapons such as `Pistols` `Mid Tier` and `Rifles` and enable auto buying of them!
  - Every team has different config for auto buy!
  - Enabled items will be highlighted in color
  - There are priority settings
  - The lower the priority, the auto buy will try to buy it before the others
  - Auto buy checks if in buy menu already, so if you refund, it will buy it back
  - Auto buy works only in freezetime, so if you need to sell something you can sell it after the freezetime

---

### Version 2.0.7 changelog
- Fix with app crashing when `steamapi.exe` cant be launched
- Added menu item in right click menu to launch `CS2` from `CSAuto`
- Added an option to shutdown `CSAuto` after exiting `CS2`
- Using raw data to load changelog and version, github api limits shouldn't be hit now
- `CSAuto` now checks whether your `Windows` installation is in dark theme and if so applies dark theme to it self as well
- Language files are being compressed with [`GZip`](https://en.wikipedia.org/wiki/Gzip) compression to save space

## What's Changed
* Add FullChangelog and Version + GH Actions by @NoPlagiarism in https://github.com/MurkyYT/CSAuto/pull/26

---

### Version 2.0.6 changelog
- Optimzation of program idle state
- Fix with parsing GSI file
- Discord RPC now updates everything when in game (details and state)
- Fix with Discord RPC which crashes when there are more than 128 characters in the state or details
- Discord customization GUI now has the same limit of 128 characters
- Added current player `Name` `Kills` `Deaths` and `MVPS's` to discord rich presence templates
- Saving to error log when there is an exception on startup so it would be easier to investigate the issue
- Updated startup error message 
- Looks like the old way of capturing screen works, the app defaults to the new one, if the new one does not work try to enable the old way
- Languages are now saved in .txt files in a folder named `resource`
- New launch option
  - `--language [language name]` - Change app language to the one specifed, doesn't change the settings
- Optimized memory and cpu usage even more
- Auto buy now should work better after half change and on match start

---

### Version 2.0.5a changelog
- I feel very ashamed but screen capture still crashes so **now** hopefully it will not as the DXGICapture lib doesn't use pointers anymore and i did some security precautions in the app itself, if it will still crash i will probably port the native c++ code to c#, unfortunately

---

### Version 2.0.5 changelog
- Changed default intaller location because can't create files in admin protected folders
- Showing a warning message when can't create files in admin protected folders
- DXGICapture library update
- Fix #24 
- Fix #25

---

### Version 2.0.4 changelog
- Fix bug with spotify auto play
- Fix bug with color loading
- DXGICapture library update
- Added sending auto accept image debug to telegram
- Changelog now loads only when opening the changelog category

---

### Version 2.0.3 changelog
- Discord Rich Presence map image now loads from the web
- Fixed a bug with [GSI](https://developer.valvesoftware.com/wiki/Counter-Strike:_Global_Offensive_Game_State_Integration) which happens when GSI doesn't have the player category in it (for some reason?) which crashed the parsing
- Fixes #23 

---

### Version 2.0.2 changelog
- App crash handling should be better now
- Settings now are saved per-user, and not per-location + per-user
- Optimized cpu usage of aplication after gui window opening and after closing cs2 when app was running

---

### Version 2.0.1 changelog
- Fixes #22

---

### Version 2.0.0 changelog

**Counter-Strike 2 Released and so did CSAuto v2**

- In CSAuto 2.0.0 i've changed how settings work and added a gui window
- Fixed some issues with the notification icon 
- You can now customize Discord Rich Presence
- Added Buttons to Discord RPC! you can add one that you want. and there is another one which is mendatory which is a link to this repo
- Auto buy now waits untill you are not in chat/console
- Some optimizations there and here
- Unusual and rare crash fixes
- Overall better feeling and hopefully performance
- Added launch options
   - ```--show``` - use to show guiwindow on startup
   - ```--maximized``` - use to make guiwindow always open maximized
- Added new icon to application! (credit @NoPlagiarism)
- Not sure if everything works as it should and as it was earlier
- I would like you to send an issue if you encounter any bug or unusual behaviour

# ⚠️Important Notice⚠️
### You will need to set all the settings again as i added company name to the assembly!

# _Have fun playing CS2 and using CSAuto v2 :)_

## What's Changed
* Merge master and source2 by @MurkyYT in https://github.com/MurkyYT/CSAuto/pull/3
* Design overhall and GUI addition by @MurkyYT in https://github.com/MurkyYT/CSAuto/pull/15
* Add Discord RPC Customization menu by @MurkyYT in https://github.com/MurkyYT/CSAuto/pull/17
* Make GitHub logo vector by @NoPlagiarism in https://github.com/MurkyYT/CSAuto/pull/19
* Redisigned logo by @NoPlagiarism in https://github.com/MurkyYT/CSAuto/pull/21
* Getting CSAuto to work with Counter-Strike 2 by @MurkyYT in https://github.com/MurkyYT/CSAuto/pull/14

---

### 2.0.0-beta.6 Changelog
- Loading button colors from web, if cant load latest colors
- Some bug fixes
- Added button colors to debug window

---

### 2.0.0-beta.5 Changelog
- More fixes with DXGICapture (help)
- Should capture all exceptions now
- Fixed github logo by making it vector (@NoPlagiarism )
- Added launch options!
   - ```--show``` - use to show guiwindow on startup
   - ```--maximized``` - use to make guiwindow always open maximized
- Added discord rpc buttons templates (Steam Profile, FaceitFinder profile, CSGOStats profile)

## What's Changed
* Make GitHub logo vector by @NoPlagiarism in https://github.com/MurkyYT/CSAuto/pull/19

---

### 2.0.0-beta.4 Changelog
- Latest CS2 Update changed accept button color, corrected for that
- Fixes with frame capture again, should work flawlessly now

---

### 2.0.0-beta.3 Changelog
- Some more memory optimizations!
- Some fixes when exiting from app
- Added latest frame catpture to Debug in ui window
- App now saves error log on crash with the cause of the crash!
- Added discord rpc error logging as well
- Updated mobiles app in app title from ```CS:GO``` to ```Counter-Strike 2```

---

### 2.0.0-beta.2 Changelog
- Optimized app even more
- App should use less memory
- Fixed issues with discord rpc which will make discord rpc not visible at all
- Added customizing buttons to discord rpc
- Fixed the readme
- Some fixes with translation

---

### 2.0.0-beta.1 Changelog
- Bug fix with frame capture
- Translation fix
- GitHub icon fix
- Added buttons to discord rpc
- Changed to an actual [Discord RPC .NET Library](https://github.com/Lachee/discord-rpc-csharp)
- Changed from CS:GO icon to CS2 icon

---

### Version 2.0.0-beta changelog
**As Counter-Strike 2 is in kinda open beta, i decided to release 2.0.0-beta so that i can eliminate all bugs before cs2's full release!**
**I would like you to send an issue if you encounter any bug or unusual behaviour, thanks in advance!**
# ⚠️Important Notice⚠️
### You will need to set all the settings again as i added company name to the assembly!

---

## This is the last version that will be compatible with CS:GO

### Version 1.1.2 changelog
1. Changed how bomb notifications work in telegram (#16 )
   - Telegram bot shows only when bomb is planted without the timer
## What's Changed
* VirusTotal detection fixes + new class for api keys by @MurkyYT in https://github.com/MurkyYT/CSAuto/pull/12
* Fix telegram notifications + compilation fix by @MurkyYT in https://github.com/MurkyYT/CSAuto/pull/13

---

### Version 1.1.1 changelog
1. Languages now work differently in app
2. Checking if really accepted match
3. Added Telegram notifications!
4. Fixed bomb notifications

---

### Version 1.1.0 changelog
1. Added mobile notification menu where you can disable different notifications
2. Changed from searching everytime for csgo process to finding it once
3. Added a new notification to mobile, bomb status, you can see time left till explosion, bomb has been defused, or the bomb exploded
4. Added translations to notifications
5. Added translations to exceptions
6. There should be no more ui stopping functions
7. When the language isnt found, try to switch to english, if english isnt found stay with the category
8. Added steamworks api to get amount of players in csgo lobby (you have to turn it on manually, could be bannable, enable at your own risk!)

## What's Changed
* Moved all csauto project to a folder by @MurkyYT in https://github.com/MurkyYT/CSAuto/pull/9
* Improvments of application by @MurkyYT in https://github.com/MurkyYT/CSAuto/pull/10

---

### Version 1.0.9 changelog
1. Added languages to the application, available languages right now are: English and Russian.
2. Added icon to the installer (thanks @NoPlagiarism)
3. There are now [contributors](https://github.com/MurkyYT/CSAuto/#contributors) in the README
4. Added [building instruction](https://github.com/MurkyYT/CSAuto/#building-the-app) to the README
5. Added [russian translation](https://github.com/MurkyYT/CSAuto/blob/master/README_ru.md) to the README
6. Added [FAQ](https://github.com/MurkyYT/CSAuto/#faq) to the README
## What's Changed
* Some ru&en lang fix + "restartneeded" by @NoPlagiarism in https://github.com/MurkyYT/CSAuto/pull/8

## New Contributors
* @NoPlagiarism made their first contribution in https://github.com/MurkyYT/CSAuto/pull/8

---

### Version 1.0.8 changelog
1. Discord RPC and GameState Server are now started when cs is started and disabled when cs is closed
2. Discord RPC shows icon now for only supported maps
3. Added function caller to log
4. Added spectating/watching gotv to discord rpc
5. Optimized discord rpc update
6. Discord rpc shows score by first showing the score of your team
7. Created a mobile app on android which sends you a notification when CSAuto on desktop
- Accepted a match
- Found out that u are in lobby
- Found out you loaded on to a map
8. To connect to the mobile app you have to download the app, install it, then start the server and allow the app to ignore battery optimization.
After you have done that you will see a notification with your ip address, in the desktop app go to "Mobile App", enable it, and then select "Enter Ip Address", you should see an input box, in the input box enter the ip address you see in the notification.
After that you should be good to go.

---

### Version 1.0.7 changelog
1. Made button search on all screen height and not half of it
2. After pressing the accept button the app now waits 20 seconds before starting to search again
3. Added discord rpc logging
4. Various bug fixes and quality of life improvements

---

### Version 1.0.6 changelog
1. When auto buying stuff chat and console are closed
2. Discord rpc now shows csgo friendcode and not steamid
3. Unfortunately the app is now more then just an executable
4. Various bug fixes

---

### Version 1.0.5a changes
1. Discord rpc now shows time elapsed from match start/time in lobby
2. Discord rpc shows round state in the place where time elapsed was before, if in warmup it shows 'Warmup'

---

### Version 1.0.5 fixes
1. Using latest release github api to find the latest tag

---

### Version 1.0.4 updates
1. Discord RPC now shows time elapsed from match started
### Version 1.0.4 fixes
1. Fixed how the app is checking if the player is in game by checking if the map is found
2. Fixed Discord RPC flickering and not showing correctly
3. Fixed Discord RPC timestamp for time elapsed
4. Fixed checking for updates by using github's api

---

### Version 1.0.3 updates
1. Added DiscordRPC implementation
2. You can now open gsi output text to parse it
3. You can save gsi output

---

### Version 1.0.2a updates
1. You have an option to enable checking for updates on startup
2. All the automation menu items are now in its own category
3. All settings are now in its own category
4. Added open debug window option to the debug category
### Version 1.0.2a fixes
1. Fixed the title in the Debug window from "CSAUto" to "CSAuto"
2. Fixed the log time format

---

### Version 1.0.2 updates
1. Added automatic resuming or pausing of spotify song (when you are alive the song is paused)

---

### Version 1.0.1 fixes
1. Fixed a bug when logging wont turn on with app on windows startup
2. App now shows a correct error message when CS:GO or Steam path isn't found
3. If CS:GO or Steam path isn't found disable Auto Buy and Auto Reload options as it is dependant on [CS:GO GSI](https://developer.valvesoftware.com/wiki/Counter-Strike:_Global_Offensive_Game_State_Integration)

---


